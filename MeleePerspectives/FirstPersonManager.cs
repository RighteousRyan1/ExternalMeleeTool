using ExternalMeleeTool;
using ExternalMeleeTool.GameComponents;
using ExternalMeleeTool.Melee;
using ExternalMeleeTool.Melee.Collision;
using ExternalMeleeTool.Melee.Fighter;
using ExternalMeleeTool.Melee.HSD;
using ExternalMeleeTool.Utilities;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace MeleePerspectives; 
// idea: make camera snap to pk thunder
public static class FirstPersonManager {
    public static ConsoleKey motionSickKey = ConsoleKey.T;
    public static ConsoleKey fovUp = ConsoleKey.OemPlus;
    public static ConsoleKey fovDown = ConsoleKey.OemMinus;
    public static ConsoleKey faceToggle = ConsoleKey.Delete;

    public static bool MotionSickReduce;
    public static float FovDeg = 110.0f;
    public static bool HideFace = true;
    public static bool IsEnabled = false;

    static bool[] ftFaceHidden = new bool[4];
    static bool[] ftAltFaceHidden = new bool[4];

    static readonly Dictionary<FtKind, int> _stupidAssCharacterMapping = new() {
        [FtKind.Ganondorf] = 59,
        [FtKind.Pikachu] = 14,
        [FtKind.Pichu] = 20,
        [FtKind.Zelda] = 95
    };

    static readonly FtKind[] _ftUseHip = [FtKind.Marth, FtKind.Roy, FtKind.DonkeyKong, /* i think the rats need to use something different, like XRotN */ FtKind.Pikachu, FtKind.Pichu];
    // wacky ass shit: you can notice weird... inconsistencies in transformations (on the y axis, i guess).
    // if young link hangs from ledge, camera points correctly. if he's doing his "foot tapping" idle animation, the camera points upward when it should be DOWNWARD.
    // how tf does one fix this? I can't put him in the _ftInverseYRot section because then it fucks up his ledge hang look
    static readonly FtKind[] _ftInverseYRot = [FtKind.Mewtwo, FtKind.DonkeyKong, 
        FtKind.Marth, FtKind.Popo, FtKind.Pichu, FtKind.Roy];
    static readonly FtKind[] _ftDirIndepend = [FtKind.Zelda, FtKind.Ness, FtKind.Yoshi]; // sheik is funky. come back later

    /// <summary>Hides the player's face (largely assuming neutral skins), if not already hidden.</summary>
    public static void PlayerFaceHide(FighterData fighter) {
        if (fighter.IsTransformed) {
            if (ftAltFaceHidden[fighter.Port]) {
                return;
            }
        }
        else if (ftFaceHidden[fighter.Port]) return;
        if (fighter.Position == Vector3.Zero) return;
        if (fighter.DObjs.data == 0) return;
        // the default anim state
        if (fighter.AnimState < 0 || fighter.AnimState > FtAnimState.Count) return;

        var dobjList = fighter.DObjs.data; // DObj**

        var mappingExists = HeadHideIndices.TryGetValue(fighter.CharKind, out var dobjs);

        // iterates the parent jobj's dobjs
        // maybe to properly apply the head rotation (like when DJ-ing), i need to apply the rotation of a sooner
        // fighter part to get basically a "base rotation"? like NeckN? or XRotN/YRotN?
        if (mappingExists) {
            // could there be a way to hide dobjs that are at the head? is that logically possible?
            // maybe use frustum intersection?
            for (int i = 0; i < dobjs!.Length; i++) {
                var dobj_ptr = Dolphinterop.ReadPtr(dobjList + dobjs[i] * 4);

                if (dobj_ptr == 0) continue;

                var dobj = Dolphinterop.Read<DObj>(dobj_ptr);
                var pobj = Dolphinterop.Read<PObj>(dobj.pobj);

                // hide the pobj and the dobj (in case there's vertex colors)
                pobj.flags |= PObjFlags.CullBack | PObjFlags.CullFront;
                dobj.flags &= ~DObjFlags.Visible;

                Dolphinterop.Write(dobj_ptr, dobj);
                Dolphinterop.Write(dobj.pobj, pobj);
            }
        }

        if (fighter.IsTransformed) ftAltFaceHidden[fighter.Port] = true;
        else ftFaceHidden[fighter.Port] = true;
    }
    // this is really scary. this method causes a segfault in melee when used on Mainline, but not Ishiiruka.
    public static void PlayerDObjRestore(FighterData fighter) {
        if (!ftFaceHidden[fighter.Port]) return;

        foreach (var dobj_desc in UnsafeUtils.IteratePointerList<DObj>(fighter.DObjs.data, d => d.next)) {
            if (dobj_desc.Ptr == 0) continue;

            var targetDobj = dobj_desc.Ptr.As<DObj>();
            // hide the pobj and the dobj (in case there's vertex colors)
            //targetDobj.flags = 0;
            targetDobj.flags |= DObjFlags.Visible;
            Dolphinterop.Write(dobj_desc.Ptr, targetDobj);

            if (dobj_desc.Value.pobj != 0) {
                var pobj = targetDobj.pobj.As<PObj>();

                // pobj.flags &= ~(PObjFlags.CullBack | PObjFlags.CullFront);
                pobj.flags &= ~PObjFlags.CullBack;             
                // pobj.flags |= PObjFlags.CullBack;

                // this pobj write is causing the panic on mainline. only if i set the flags above ^
                // Dolphinterop.WriteU16(targetDobj.pobj + 0xC, (ushort)pobj.flags);
                Dolphinterop.Write(targetDobj.pobj, pobj);
            }
        }

        if (fighter.IsTransformed) ftAltFaceHidden[fighter.Port] = false;
        else ftFaceHidden[fighter.Port] = false;
    }
    public static bool GetHeadParametersForStupidCharacters(int ply, out Vector3 position, out Vector3 lookAt, out FighterBone ftBone) {
        var fd = MeleeCamManip.Match.Fighters[ply];

        position = lookAt = default;
        ftBone = default;

        if (!_stupidAssCharacterMapping.TryGetValue(fd.CharKind, out int ftBoneIndex)) return false;

        ftBone = fd.GetUnmappedBone(ftBoneIndex);
        var jobj = Dolphinterop.Read<JObj>(ftBone.jobj);

        position = jobj.mtx.Translation;

        var overall = jobj.mtx.Rotation;

        if (float.IsNaN(overall.X)) return true;

        // maybe use -UnitZ?
        var meleeForward = Vector3.Transform(Vector3.UnitZ, overall);

        if (_ftInverseYRot.Contains(fd.CharKind)) meleeForward.Y *= -1;
        if (!_ftDirIndepend.Contains(fd.CharKind)) meleeForward.Y *= fd.Direction;
        // flip z for melee space
        // -meleeForward.Y ??? multiply yes or no???
        var camForward = new Vector3(-meleeForward.X, -meleeForward.Y, -meleeForward.Z); // some character have conflicts with multiplying y forward by direction???

        lookAt = position + camForward;

        // Console.WriteLine(ftBone.mtx);

        return true;
    }
    public static void GetHeadParameters(int ply, out Vector3 position, out Vector3 lookAt, out FighterBone ftBone, out FighterHurtCapsule hc) {
        var fd = MeleeCamManip.Match.Fighters[ply];
        hc = new FighterHurtCapsule();
        var part = FtPart.Invalid;
        // int numInvalids = 0;
        position = lookAt = default;
        // find head hitbox
        // Console.WriteLine("Char " + fd.CharKind + ":\n");
        for (int i = 0; i < FighterData.FighterHurtCapsuleBuffer15.LENGTH; i++) {
            var hurt = fd.Hurtboxes[i];

            var partId = fd.GetPartFromBoneIndex(hurt.capsule.bone_idx);
            // puff, kirby, any ball characters
            if (partId == FtPart.WaistN) {
                hc = hurt;
                part = partId;
                // dont break because we defer to WaistN only if there is no RShoulderN
            }
            // any character without any weirdness
            if (partId == FtPart.HeadN) {
                hc = hurt;
                part = partId;
                break;
            }
        }

        
        ftBone = fd.GetBone(part);

        var jobj = Dolphinterop.Read<JObj>(ftBone.jobj);
        var rotquat = jobj.mtx.Rotation;

        if (float.IsNaN(rotquat.X)) return;

        // center of head hurtbox
        var pos = (hc.capsule.start + hc.capsule.end) / 2;

        // invert camera eye for melee coordinates
        position = new Vector3(pos.X, pos.Y, -pos.Z);

        // var neckT = GetTransformOf(fd, FtPart.NeckN);
        // maybe use -UnitZ?
        var headForw = Vector3.Transform(Vector3.UnitZ, rotquat);
        // var neckForw = Vector3.Transform(Vector3.UnitZ, neckT.Rotation);

        // Console.WriteLine(fd.GetActionNameFull(fd.ActionId));

        // flip z for melee space
        // -meleeForward.Y ??? multiply yes or no???
        if (_ftInverseYRot.Contains(fd.CharKind)) headForw.Y *= -1;
        if (!_ftDirIndepend.Contains(fd.CharKind)) headForw.Y *= fd.Direction;
        // if (_ftDirFlip.Contains(fd.CharKind)) meleeForward.Y *= -fd.Direction;

        // new Vector3(-headForw.X, -headForw.Y, -headForw.Z)
        var camForward = -headForw; //* -neckForw; // some character have conflicts with multiplying y forward by direction???

        // cam up should just be the direction of the capsule. end - start.
        lookAt = position + camForward;
    }

    public static Matrix3x4 GetTransformOf(FighterData fighter, FtPart part) {
        var bone = fighter.GetBone(part);

        if (bone.jobj == 0) return Matrix3x4.Identity;

        return bone.jobj.As<JObj>().mtx;
    }

    public static void Update(FighterData fd, SceneData scDat, MatchData match, int port) {
        if (!scDat.IsIngame && !scDat.IsSlippiReplay) {
            for (int i = 0; i < 4; i++) {
                ftFaceHidden[i] = false;
                ftAltFaceHidden[i] = false;
            }
            return;
        }
        MeleeFreeCamera fpcam = new();

        // get real bone with GetUnmappedBone(boneIdx)
        bool isStupid = GetHeadParametersForStupidCharacters(port, out Vector3 pos, out Vector3 lookAt, out var head);
        if (!isStupid) GetHeadParameters(port, out pos, out lookAt, out head, out var hc);

        if (!MotionSickReduce) {
            // only do this special stuff for non-balloons
            if (fd.CharKind != FtKind.Jigglypuff && fd.CharKind != FtKind.Kirby) {
                // insane motion sickness. add config to reduce it?
                Camera.QuickManip((ref CObj cobj) => {
                    cobj.flags |= CObjFlags.UseUp;
                    // maybe: perform calculation to find which is lower between waist and hip? idk.
                    // neck, hip, what? all of them appear to have their inconsistencies
                    var useHip = _ftUseHip.Contains(fd.CharKind);
                    var hip = fd.GetBone(useHip ? FtPart.HipN : FtPart.NeckN);
                    var basej = hip.jobj.As<JObj>();
                    var headj = head.jobj.As<JObj>();

                    // should probably opt for local transforms... however that may be done
                    var invhj = headj.mtx.Translation * new Vector3(1, 1, -1);
                    var invbj = basej.mtx.Translation * new Vector3(1, 1, -1);

                    cobj.up = Vector3.Normalize(invhj - invbj); // z doesn't seem to affect anything.
                });
            }
            else {
                Camera.QuickManip((ref CObj cobj) => {
                    cobj.flags |= CObjFlags.UseUp;
                    cobj.up = Vector3.UnitY;
                });
            }
        }

        // do only after a second of ingame?
        if (HideFace)
            PlayerFaceHide(fd);

        // restore all other fighter heads
        foreach (var f in match.ActiveFighters) {
            if (f.Port == fd.Port) continue;

            PlayerDObjRestore(f);
        }

        fpcam.Fov = FovDeg;
        fpcam.Eye = pos;
        fpcam.Focus = lookAt;
        fpcam.ApplyToMelee();
    }

    // Here starts horrendousness. HAL freaking laboraties could not be bothered to make good systems, so some DObjs are 
    // maybe if dobjs had some sort of positioning i could hide them if they are in the camera frustum
    public static readonly Dictionary<FtKind, int[]> HeadHideIndices = new() {
        [FtKind.Fox] = [14, 15, 16, 17, 18, 20, 21, 22, 23, 44],
        [FtKind.CaptainFalcon] = [19, 20, 21, 22, 23, 24, 26, 27, 58], // 59 = neck
        [FtKind.Marth] = [20, 21, 22, 23, 24, 25, 26, 28, 29, 30, 76, 77],
        [FtKind.Zelda] = [18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 34, 53, 54],
        [FtKind.Luigi] = [9, 10, 11, 12, 13, 14, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 52],
        [FtKind.Falco] = [13, 14, 15, 16, 17, 18, 19, 35],
        [FtKind.Mario] = [13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 58],
        [FtKind.Ganondorf] = [20, 21, 22, 23, 24, 25, 26, 27, 31, 35, 36, 37, 68, 79, 80, 81],
        [FtKind.Bowser] = [16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 108],
        [FtKind.Sheik] = [16, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 55, 56, 57, 58],
        [FtKind.Ness] = [2, 3, 4, 7, 8, 9, 10, 11],
        [FtKind.Samus] = [0, 1, 2, 4, 7, 19, 25, 72, 73],
        [FtKind.Roy] = [15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35],
        [FtKind.Link] = [9, 10, 11, 12, 14, 15, 17, 18, 19, 20],
        [FtKind.YoungLink] = [3, 4, 5, 6, 8, 9, 10, 11, 12, 13],
        [FtKind.Jigglypuff] = [0, 2, 3, 4, 13],
        [FtKind.Kirby] = [0, 1, 2, 3, 4, 5, 8],
        [FtKind.Peach] = [4, 5, 6, 7, 8, 9, 10, 11, 12, 14, 15, 17],
        [FtKind.Yoshi] = [6, 7, 9, 10, 11, 12, 13, 14, 16, 17, 22, 23, 24, 36, 39], // Re and Nr have different dobj mappings. WTF HAL
        [FtKind.DrMario] = [13, 15, 17, 19, 20, 22, 21, 23, 53, 54, 55, 56, 57],
        [FtKind.DonkeyKong] = [7, 8, 13, 14, 15, 16, 17, 18, 19, 36],
        [FtKind.Pikachu] = [3, 4, 5, 6, 9, 10, 12],
        [FtKind.Pichu] = [2, 3, 4, 5, 6, 11, 17],
        [FtKind.Popo] = [0, 1, 2, 3, 6],
        [FtKind.Mewtwo] = [1, 2, 4, 7, 12, 13, 14]
    };
}
