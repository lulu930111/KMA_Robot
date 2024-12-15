using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TriLibCore.SFB;
using System.IO;
using System.Linq;

public class KRGameMangager : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup CG_System;
    public InputField INP_FileName;
    public List<Text> listDirPathText;
    public List<Button> listDirButton;
    public List<Image> listOutputImage;

    [Header("Runtime Data")]
    [SerializeField] string fileName;
    [SerializeField] List<string> listDirPath;
    [SerializeField] List<Texture2D> createdTexture;
    [SerializeField] List<Sprite> createdSprite;
    
    private readonly string[] VALID_IMAGE_EXTENSIONS = { ".png", ".jpg", ".jpeg", ".bmp", ".tga" };

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
        
        for (int i = 0; i < listDirPath.Count; i++)
        {
            try {
                string dirPath = listDirPath[i];
                string file = Path.Combine(dirPath, _fileName);
                
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

    void OnDestroy()
    {
        ClearPreviousResources();
    }
}

