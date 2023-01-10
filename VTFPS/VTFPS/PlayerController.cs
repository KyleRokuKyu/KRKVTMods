using UnityEngine;
using VoxelTycoon;

namespace VTFPS
{
    class PlayerController : MonoBehaviour
    {
        public float acceleration = 10;
        public float mass = 1;
        public float jumpForce = 100;
        public float lookRate = 0.1f;

        private Vector2 prevMouse;
        private float lookAngle = 0;

        void Start ()
        {
            Xyz position;
            Voxel voxel;
            GameObject go;
            for (int x = -100; x < 100; x++)
            {
                for (int y = -100; y < 100; y++)
                {
                    voxel = Manager<WorldManager>.Current.GetSurfaceVoxel(new Xz(x, y), out position);
                    if (position != null)
                    {
                        go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        go.transform.position = new Vector3(position.X, position.Y, position.Z);
                        Destroy(go.GetComponent<MeshRenderer>());
                        Destroy(go.GetComponent<MeshFilter>());
                    }
                }
            }
            transform.position = new Vector3(0,15,0);
            Camera.main.transform.SetParent(transform);
            Camera.main.transform.localPosition = new Vector3(0,0.5f,0);
            Camera.main.transform.localRotation = Quaternion.identity;
        }

        void Update ()
        {
            if (Input.GetKey(KeyCode.W))
            {
                transform.localPosition += Planar(transform.forward * acceleration * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.S))
            {
                transform.localPosition += Planar(-transform.forward * acceleration * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.A))
            {
                transform.localPosition += Planar(-transform.right * acceleration * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.D))
            {
                transform.localPosition += Planar(transform.right * acceleration * Time.deltaTime);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                transform.localPosition += (Vector3.up * jumpForce * Time.deltaTime);
            }

            if (!Physics.Raycast(new Ray(transform.position, Vector3.down*0.5f))) {
                transform.localPosition -= new Vector3(0, 100f * Time.deltaTime, 0);
            }

            transform.localEulerAngles += new Vector3(0, (Input.mousePosition.x - prevMouse.x) * lookRate, 0);
            lookAngle -= (Input.mousePosition.y - prevMouse.y) * lookRate;
            if (lookAngle > 180)
            {
                lookAngle -= 360f;
            }
            if (lookAngle <= -180)
            {
                lookAngle += 360f;
            }
            lookAngle = Mathf.Clamp(lookAngle, -89, 89);
            Camera.main.transform.localEulerAngles = new Vector3(lookAngle, 0, 0);
            prevMouse = Input.mousePosition;
        }

        void LateUpdate ()
        {
            Camera.main.transform.localPosition = new Vector3(0, 0.5f, 0);
            Camera.main.transform.localEulerAngles = new Vector3(lookAngle, 0, 0);
        }

        Vector3 Planar (Vector3 vec)
        {
            return new Vector3(vec.x, 0, vec.z);
        }
    }
}
