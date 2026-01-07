from moviepy.editor import VideoFileClip

input_video = "/Users/aleksejspiscevs/Downloads/exp33.mp4"   # path to the file you downloaded from YouTube Studio
output_audio = "exp33.mp3"  # desired mp3 filename

# Load the video
video_clip = VideoFileClip(input_video)

# Extract and write the audio as mp3
audio_clip = video_clip.audio
audio_clip.write_audiofile(output_audio)

# Close clips to release resources
audio_clip.close()
video_clip.close()

print("Done! Saved as", output_audio)
