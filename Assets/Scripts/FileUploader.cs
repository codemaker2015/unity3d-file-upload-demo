using System.Collections;
using System.IO;
using SFB;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class FileUploader : MonoBehaviour {
    public Text filePathText;
    public RawImage imageDisplay;
    public AudioSource audioSource;
    public RawImage videoDisplay; 
    public VideoPlayer videoPlayer;

    public void OpenFileBrowser () {
        var extensions = new [] {
            new ExtensionFilter ("All Files", "*.*"),
            new ExtensionFilter ("Images", "jpg", "png"),
            new ExtensionFilter ("Audio", "mp3"),
            new ExtensionFilter ("Video", "mp4")
        };

        var paths = StandaloneFileBrowser.OpenFilePanel ("Open File", "", extensions, false);

        if (paths.Length > 0) {
            string path = paths[0];
            filePathText.text = path;
            StartCoroutine (SaveFile (path));
        }
    }

    private IEnumerator SaveFile (string filePath) {
        string fileName = Path.GetFileName (filePath);
        string destinationPath = Path.Combine (Application.persistentDataPath, fileName);

        if (File.Exists (destinationPath)) {
            File.Delete (destinationPath);
        }

        File.Copy (filePath, destinationPath);

        yield return null;

        Debug.Log ($"File saved to: {destinationPath}");

        // Optionally, load the file into the scene
        LoadFile (destinationPath);
    }

    private void LoadFile (string filePath) {
        string extension = Path.GetExtension (filePath).ToLower ();

        if (extension == ".jpg" || extension == ".png") {
            DisplayImage (filePath);
        } else if (extension == ".mp3") {
            PlayAudio (filePath);
        } else if (extension == ".mp4") {
            PlayVideo (filePath);
        } else {
            Debug.Log ("Unsupported file type.");
        }
    }
    private void DisplayImage (string path) {
        if (imageDisplay == null) {
            Debug.LogError ("No RawImage assigned for displaying images.");
            return;
        }

        Texture2D texture = LoadTexture (path);
        if (texture != null) {
            imageDisplay.texture = texture;
        }
    }

    private Texture2D LoadTexture (string path) {
        byte[] fileData = File.ReadAllBytes (path);
        Texture2D texture = new Texture2D (2, 2);
        texture.LoadImage (fileData);
        return texture;
    }

    private void PlayAudio (string path) {
        if (audioSource == null) {
            Debug.LogError ("No AudioSource assigned for playing audio.");
            return;
        }

        StartCoroutine (PlayAudioCoroutine (path));
    }

    private IEnumerator PlayAudioCoroutine (string path) {
        WWW www = new WWW ("file://" + path);
        yield return www;

        AudioClip clip = www.GetAudioClip (false, true);
        audioSource.clip = clip;
        audioSource.Play ();
    }

    private void PlayVideo (string path) {
        if (videoDisplay == null || videoPlayer == null) {
            Debug.LogError ("RawImage or VideoPlayer is not assigned.");
            return;
        }

        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = path;
        videoPlayer.Prepare ();

        videoPlayer.prepareCompleted += (source) => {
            videoDisplay.texture = videoPlayer.texture;
            videoPlayer.Play ();
        };
    }
}