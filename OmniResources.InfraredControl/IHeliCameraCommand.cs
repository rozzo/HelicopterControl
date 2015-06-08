namespace OmniResources.InfraredControl
{
    /// <summary>
    /// Defines commands for taking pictures/video
    /// </summary>
    public interface IHeliCameraCommand
    {
        /// <summary>
        /// Gets or sets a value indicating whether to take a picture (S107-C)
        /// </summary>
        bool TakePicture { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to take a video (S107-C)
        /// </summary>
        bool TakeVideo { get; set; }
    }
}
