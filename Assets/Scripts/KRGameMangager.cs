using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TriLibCore.SFB;
using System.IO;
using System.Linq;
using RootMotion.Demos;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class KRGameMangager : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup CG_System;
    public InputField INP_FileName;
    public InputField INP_ImageExtension;
    public InputField INP_SoundExtension;
    public List<Text> listDirPathText;
    public List<Button> listDirButton;
    public List<Image> listOutputImage;
    public AudioSource AUD_Sound;

    [Header("Runtime Data")]
    [SerializeField] string fileName;
    [SerializeField] string imageExtension;
    [SerializeField] string soundExtension;
    [SerializeField] List<string> listDirPath;
    [SerializeField] List<Texture2D> createdTexture;
    [SerializeField] List<Sprite> createdSprite;
    [SerializeField] List<AudioClip> createdAudioClips = new List<AudioClip>();
    
    private readonly string[] VALID_IMAGE_EXTENSIONS = { ".png", ".jpg", ".jpeg", ".bmp", ".tga" };
    private readonly Dictionary<string, AudioType> AUDIO_TYPE_MAP = new Dictionary<string, AudioType>
    {
        {".wav", AudioType.WAV},
        {".mp3", AudioType.MPEG},
        {".ogg", AudioType.OGGVORBIS}
    };

    void Start()
    {
        listDirPath = new List<string>();
        createdTexture = new List<Texture2D>();
        createdSprite = new List<Sprite>();
        for (int i = 0; i < listDirPathText.Count; i++)
        {
            listDirPath.Add(SystemConfig.Instance.GetData<string>($"dir{i}", "Null"));
            listDirPathText[i].text = listDirPath[i];
        }

        INP_ImageExtension.onValueChanged.AddListener(OnChangeImageExtension);  
        INP_ImageExtension.text = SystemConfig.Instance.GetData<string>("imgExt", "Null");
        INP_SoundExtension.onValueChanged.AddListener(OnChangeSoundExtension);
        INP_SoundExtension.text = SystemConfig.Instance.GetData<string>("sndExt", "Null");
        INP_FileName.onValueChanged.AddListener(OnChangeFileName);
        INP_FileName.text = SystemConfig.Instance.GetData<string>("FileName", "Null");

        for (int i = 0; i < listDirButton.Count; i++)
        {
            int index = i;
            listDirButton[i].onClick.AddListener(() => SetupDictionary(index));
        }
    }

    void Update(){
        if (Input.GetKeyDown(KeyCode.F8))
        {
            CG_System.blocksRaycasts = !CG_System.blocksRaycasts;
            CG_System.alpha = CG_System.alpha == 1 ? 0 : 1;
        }
    }

    void OnChangeFileName(string _fileName){
        SystemConfig.Instance.SaveData("FileName", _fileName);
        fileName = _fileName;

        LoadImages(fileName);
        LoadSound(fileName);
    }

    void OnChangeImageExtension(string _imageExtension){
        SystemConfig.Instance.SaveData("imgExt", _imageExtension);
        imageExtension = _imageExtension;
    }

    void OnChangeSoundExtension(string _soundExtension){
        SystemConfig.Instance.SaveData("sndExt", _soundExtension);
        soundExtension = _soundExtension;
    }

    void SetupDictionary(int index)
    {
        var result = StandaloneFileBrowser.OpenFolderPanel("Open Path", Application.dataPath, false);
        if (result != null)
        {
            var hasFiles = result.Count > 0 && result[0].HasData;

            if (hasFiles)
            {
                string dirPath = result[0].Name;

                if (listDirPathText[index]) listDirPathText[index].text = dirPath;
                SystemConfig.Instance.SaveData($"dir{index}", dirPath);
                listDirPath[index] = dirPath;
                Debug.Log("Success");
            }
        }
    }

    void LoadImages(string _fileName){
        ClearPreviousResources();
        
        for (int i = 0; i < listOutputImage.Count; i++)
        {
            try {
                string dirPath = listDirPath[i];
                string file = Path.Combine(dirPath, $"{_fileName}.{imageExtension}");
                
                if (File.Exists(file)) {
                    listOutputImage[i].sprite = LoadSprite(file);
                } else {
                    Debug.LogWarning($"File not found: {file}");
                    listOutputImage[i].sprite = null;
                }
            }
            catch (System.Exception e) {
                Debug.LogError($"Error loading image {i}: {e.Message}");
                listOutputImage[i].sprite = null;
            }
        }
    }

    Sprite LoadSprite(string filePath){
        try {
            string extension = Path.GetExtension(filePath).ToLower();
            if (!VALID_IMAGE_EXTENSIONS.Contains(extension))
            {
                Debug.LogError($"Invalid image format: {extension}. Supported formats: {string.Join(", ", VALID_IMAGE_EXTENSIONS)}");
                return null;
            }

            byte[] fileData = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2);
            if (!texture.LoadImage(fileData))
            {
                Debug.LogError($"Failed to load image: {filePath}");
                Destroy(texture);
                return null;
            }
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            createdSprite.Add(sprite);
            createdTexture.Add(texture);
            return sprite;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in LoadSprite: {e.Message}");
            return null;
        }
    }

    private async void LoadSound(string _fileName){
        string dirPath = listDirPath.Last();
        if (dirPath == "Null")
        {
            Debug.LogWarning("Sound directory path not set");
            return;
        }

        string file = Path.Combine(dirPath, $"{_fileName}.{soundExtension}");
        if (File.Exists(file)) {
            AUD_Sound.clip = await LoadAudioClipAsync(file);
            if (AUD_Sound.clip != null)
            {
                createdAudioClips.Add(AUD_Sound.clip);
                AUD_Sound.Play();
            }
        } else {
            AUD_Sound.Stop();
        }
    }

    private async Task<AudioClip> LoadAudioClipAsync(string filePath)
    {
        try {
            string extension = Path.GetExtension(filePath).ToLower();
            if (!AUDIO_TYPE_MAP.ContainsKey(extension))
            {
                Debug.LogError($"Unsupported audio format: {extension}. Supported formats: {string.Join(", ", AUDIO_TYPE_MAP.Keys)}");
                return null;
            }

            string url = "file://" + filePath;
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AUDIO_TYPE_MAP[extension]))
            {
                var operation = www.SendWebRequest();
                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (www.result == UnityWebRequest.Result.Success)
                {
                    return DownloadHandlerAudioClip.GetContent(www);
                }
                else
                {
                    Debug.LogError($"Failed to load audio: {www.error}");
                    return null;
                }
            }
        }
        catch (System.Exception e) {
            Debug.LogError($"Error in LoadAudioClipAsync: {e.Message}");
            return null;
        }
    }

    private void ClearPreviousResources()
    {
        foreach (var texture in createdTexture)
        {
            if (texture != null)
            {
                Destroy(texture);
            }
        }
        foreach (var sprite in createdSprite)
        {
            if (sprite != null)
            {
                Destroy(sprite);
            }
        }
        createdTexture.Clear();
        createdSprite.Clear();

        foreach (var clip in createdAudioClips)
        {
            if (clip != null)
            {
                Destroy(clip);
            }
        }
        createdAudioClips.Clear();
    }

    void OnDestroy()
    {
        ClearPreviousResources();
    }
}

