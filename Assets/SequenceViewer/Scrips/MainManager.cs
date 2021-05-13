using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TriLibCore;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{
    public List<ModelSequenceItem> ModelSequenceItemList;
    public string folderPath;
    public bool isPlaying;
    public bool isOpend;
    public int currentFrameIndex;
    public int frameRate;
    private float interval;
    private AssetLoaderOptions assetLoaderOptions;
    public Text frameText;
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;

    public bool needBaseMap;
    public bool needMaskMap;
    public bool needNormalMap;

    public string baseMapAdditionName;
    public string maskMapAdditionName;
    public string normalMapAdditionName;

    public int fileMaxSize;
    public InputField inputField;
    public InputField frameRateInputField;
    public GameObject loadingUI;
    public Text loadingText;
    public Slider slider;

    public bool isSliderSelect;

    // Start is called before the first frame update
    void Start()
    {
        
        assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions();
        ModelSequenceItemList = new List<ModelSequenceItem>();

        inputField.onValueChanged.AddListener((str)=> {
            folderPath = str;
        });

        frameRateInputField.onValueChanged.AddListener((value) => {
            try
            {
                frameRate = int.Parse(value);
            }
            catch { 
            
            }
        });

        slider.onValueChanged.AddListener((value) => {
            if (isOpend&& isSliderSelect)
            {
                if (isPlaying)
                {
                    isPlaying = !isPlaying;
                }
           
                currentFrameIndex = (int)value;
                UpdateModel(currentFrameIndex);
            }
        });
        frameRate = 22;
        interval = 1f / frameRate;
    }

    public void OnClickSlider()
    {      
        isSliderSelect = true;
        if (isOpend) {
            if (isPlaying)
            {
                isPlaying = !isPlaying;
            }
         
            currentFrameIndex = (int)slider.value;
            UpdateModel(currentFrameIndex);
        }  
    }

    public void OnReleaseSlider()
    {
        isSliderSelect = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isOpend) {
            slider.maxValue = fileMaxSize - 1;
        }
    }

    public void OnClickReadFile() {
        loadingUI.SetActive(true);
        Invoke("ReadFiles",0.2f);
    }

    public void ReadFiles()
    {
        ModelSequenceItemList.Clear();
        isPlaying = false;
        isOpend = false;
        folderPath += @"\";
        string[] filesNames = Directory.GetFiles(folderPath, "*.obj");
        fileMaxSize = filesNames.Length;
        for (int i = 0; i < filesNames.Length; i++)
        {
            string objFilePath = filesNames[i];

            if (IsFileExist(objFilePath))
            {
                loadingText.text = $"¼ÓÔØÖÐ£¬ÇëÉÔµÈ£º{i}/{fileMaxSize}";
                string indexStr = objFilePath.Replace(".obj", "");
                var obj = AssetLoader.LoadModelFromFileNoThread(objFilePath);
                ModelSequenceItem modelSequenceItem = new ModelSequenceItem();
                modelSequenceItem.mesh = obj.RootGameObject.transform.GetChild(0).gameObject.GetComponent<MeshFilter>().mesh;
                modelSequenceItem.mesh.name = objFilePath;

                if (needBaseMap)
                {
                    string baseMapPath = $"{indexStr}{baseMapAdditionName}";
                    if (IsFileExist(baseMapPath))
                    {
                        StartCoroutine(DoWebRequestGetTexture(baseMapPath, (tex) =>
                        {
                            tex.name = baseMapPath;
                            modelSequenceItem.baseMap = tex;
                        }));
                    }
                }

                if (needMaskMap)
                {
                    string maskMapPath = $"{indexStr}{maskMapAdditionName}";
                    if (IsFileExist(maskMapPath))
                    {
                        StartCoroutine(DoWebRequestGetTexture(maskMapPath, (tex) => {
                            tex.name = maskMapPath;
                            modelSequenceItem.maskMap = tex;
                        }));
                    }
                }

                if (needNormalMap)
                {
                    string normalMapPath = $"{indexStr}{normalMapAdditionName}";
                    if (IsFileExist(normalMapPath))
                    {
                        StartCoroutine(DoWebRequestGetTexture(normalMapPath, (tex) => {
                            tex.name = normalMapPath;
                            modelSequenceItem.normalMap = tex;
                        }));
                    }
                }

                ModelSequenceItemList.Add(modelSequenceItem);
                Destroy(obj.RootGameObject);
            }
        }
        isOpend = true; 
        loadingUI.SetActive(false);
        PlayOrPause();
    }

    public void PlayOrPause()
    {
        if (isPlaying)
        {
            isPlaying = !isPlaying;
        }
        else
        {
            isPlaying = !isPlaying;
            Play();
        }       
    }

    public void Play()
    {
        StartCoroutine(DoPlay());
    }

    public IEnumerator DoPlay() {
        if (isOpend) {
            while (isPlaying)
            {
                interval = 1 / (float)frameRate;
                if (currentFrameIndex > ModelSequenceItemList.Count - 1)
                {
                    currentFrameIndex = 0;
                }
                UpdateModel(currentFrameIndex);
          
                currentFrameIndex += 1;
                yield return new WaitForSeconds(interval);
            }
        }    
    }

    public void UpdateModel(int index)
    {
        frameText.text = currentFrameIndex.ToString();
        slider.value = currentFrameIndex;

        meshRenderer.material.mainTexture = ModelSequenceItemList[currentFrameIndex].baseMap;
        meshRenderer.material.SetTexture("_MaskMap", ModelSequenceItemList[currentFrameIndex].maskMap);
        meshRenderer.material.SetTexture("_NormalMap", ModelSequenceItemList[currentFrameIndex].normalMap);
        meshFilter.mesh = ModelSequenceItemList[currentFrameIndex].mesh;
    }

    [System.Serializable]
    public class ModelSequenceItem
    {
        public Mesh mesh;
        public Texture baseMap;
        public Texture normalMap;
        public Texture maskMap;
    }

    public bool IsFileExist(string path)
    {
        return File.Exists(path);
    }

    IEnumerator DoWebRequestGetTexture(string url, Action<Texture> onSuccess, Action<string> onError = null)
    {
        using (UnityWebRequest unityWebRequest = UnityWebRequestTexture.GetTexture(url))
        {
            yield return unityWebRequest.SendWebRequest();

            if (unityWebRequest.result == UnityWebRequest.Result.ConnectionError || unityWebRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                //Debug.Log("error");
                if (onError != null)
                {
                    onError(unityWebRequest.error);
                }
            }
            else
            {
                DownloadHandlerTexture downloadHandlerTexture = unityWebRequest.downloadHandler as DownloadHandlerTexture;
                onSuccess(downloadHandlerTexture.texture);

            }
        }
    }

    public void Next() {
        if (isPlaying) {
            isPlaying = false;
        }
        currentFrameIndex += 1;
        if (currentFrameIndex > ModelSequenceItemList.Count - 1)
        {
            currentFrameIndex = 0;
        }
        UpdateModel(currentFrameIndex);       
    }

    public void Last()
    {
        if (isPlaying)
        {
            isPlaying = false;
        }
        currentFrameIndex -= 1;
        if (currentFrameIndex <0)
        {
            currentFrameIndex = ModelSequenceItemList.Count - 1;
        }
        UpdateModel(currentFrameIndex);       
    }
}
