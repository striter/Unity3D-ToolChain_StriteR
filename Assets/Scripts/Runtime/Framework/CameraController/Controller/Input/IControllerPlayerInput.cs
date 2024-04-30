namespace CameraController.Inputs
{
    public interface IPlayerInput
    {
        public float Pitch { get; set; }
        public float Yaw { get; set; }
        public float Pinch { get; set; }

        public void Clear()
        {
            Pitch = 0f;
            Yaw = 0f;
            Pinch = 0f;
        }
    }
}