using ExternalMeleeTool.GameComponents;
using System.Numerics;

namespace ExternalMeleeTool; 
public struct MeleeFreeCamera {
    Vector3 _eye;
    Vector3 _foc;
    float _fov;
    public Vector3 Eye {
        readonly get => _eye;
        set => _eye = value;
    }
    public Vector3 Focus {
        readonly get => _foc;
        set => _foc = value;
    }
    public float Fov {
        readonly get => _fov;
        set => _fov = value;
    }
    public readonly void ApplyToMelee() {
        Camera.SetDevelopCam(_eye, _foc, _fov);
    }
}
