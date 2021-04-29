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
    public ModelSequenceItem currentModelSequenceItem;
    public List<ModelSequenceItem> ModelSequenceItemList;
    public string folderPath;
    public bool isPlaying;
    public Material baseMaterial;
    public Transform modelPos;
    public Transform modelParent;
    public int frameCount;
    private int startIndex;
    public int currentIndex;
    public float frameRate;
    private float interval;
    private AssetLoaderOptions assetLoaderOptions;
    public Text frameText;
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    // Start is called before the first frame update
    void Start()
    {
        interval = 1f / frameRate;
        // ReadFiles
        assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions();
        ModelSequenceItemList = new List<ModelSequenceItem>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ReadFiles()
    {
        GetFirstIndex();
        Debug.LogWarning("ReadFiles");
        for (int i = startIndex; i < startIndex+frameCount; i++)
        {
            string indexStr = i.ToString().PadLeft(6,'0');
            string modelFilePath = $"{folderPath}{indexStr}.obj";
            string baseMapPath = $"{folderPath}{indexStr}_material_0_BaseMap.png";
            string maskMapPath = $"{folderPath}{indexStr}_material_0_MaskMap.png";
            string normalMapPath = $"{folderPath}{indexStr}_material_0_Normal.png";

            Debug.LogWarning(modelFilePath);
            bool isModelExist = IsFileExist(modelFilePath);
            if (isModelExist) {
                // Model
                var obj = AssetLoader.LoadModelFromFileNoThread(modelFilePath);             
               // obj.RootGameObject.transform.eulerAngles = new Vector3(180, 0, 0);
              //  obj.RootGameObject.transform.position = modelPos.transform.position;
             //   obj.RootGameObject.transform.parent = modelParent;
                ModelSequenceItem modelSequenceItem = new ModelSequenceItem();
             //   modelSequenceItem.obj = obj.RootGameObject.transform.GetChild(0).gameObject;
                //modelSequenceItem.meshRenderer = modelSequenceItem.obj.GetComponent<MeshRenderer>();
                //modelSequenceItem.meshRenderer.material = baseMaterial;
                modelSequenceItem.mesh = obj.RootGameObject.transform.GetChild(0).gameObject.GetComponent<MeshFilter>().mesh;
                modelSequenceItem.mesh.name = indexStr;


                if (IsFileExist(baseMapPath))
                {
                    StartCoroutine(DoWebRequestGetTexture(baseMapPath, (tex) => {
                        tex.name = indexStr;
                        modelSequenceItem.baseMap = tex;
                    }));
                }

                if (IsFileExist(maskMapPath))
                {
                    StartCoroutine(DoWebRequestGetTexture(maskMapPath, (tex) => {
                        tex.name = indexStr;
                        modelSequenceItem.maskMap = tex;
                    }));
                }

                if (IsFileExist(normalMapPath))
                {
                    StartCoroutine(DoWebRequestGetTexture(normalMapPath, (tex) => {
                        tex.name = indexStr;
                        modelSequenceItem.normalMap = tex;
                    }));
                }
             //   modelSequenceItem.obj.SetActive(false);
                ModelSequenceItemList.Add(modelSequenceItem);

                Destroy(obj.RootGameObject);
            }
            else {
                Debug.LogError("file not exsit");
            }
        }
        currentIndex = 0;
       // UpdateModelByIndex(currentIndex);
    }

    public void GetFirstIndex() {

        string[] txtFiles = Directory.GetFiles(folderPath);
        frameCount = txtFiles.Length / 4;

        for (int i = 0; i < 100000; i++) {
            string indexStr = i.ToString().PadLeft(6, '0');
            string modelFilePath = $"{folderPath}{indexStr}.obj";

            if (IsFileExist(modelFilePath)) {
                startIndex = i;
                Debug.LogWarning("startIndex is:"+ startIndex);
                break;
            }
            else { 
            
            }
        }
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
        while (isPlaying) {
            interval = 1 / frameRate;
            if (currentIndex> ModelSequenceItemList.Count-1) {
                currentIndex = 0;
            }
            UpdateModel(currentIndex);
            //UpdateModelByIndex(currentIndex);
            frameText.text = currentIndex.ToString();
            currentIndex += 1;
            yield return new WaitForSeconds(interval);
        }
    }

    //public void UpdateModelByIndex(int index) {
    //    //if (currentModelSequenceItem != null)
    //    //{
    //    //    currentModelSequenceItem.obj.SetActive(false);
    //    //}

    //    baseMaterial.mainTexture = ModelSequenceItemList[currentIndex].baseMap;
    //    //baseMaterial.SetTexture("_MaskMap", ModelSequenceItemList[currentIndex].maskMap);
    //    //baseMaterial.SetTexture("_NormalMap", ModelSequenceItemList[currentIndex].normalMap);
    // //   ModelSequenceItemList[currentIndex].obj.SetActive(true);
    //    currentModelSequenceItem = ModelSequenceItemList[currentIndex];
    //}

    public void UpdateModel(int index)
    {
        meshRenderer.material.mainTexture = ModelSequenceItemList[currentIndex].baseMap;
        meshRenderer.material.SetTexture("_MaskMap", ModelSequenceItemList[currentIndex].maskMap);
        meshRenderer.material.SetTexture("_NormalMap", ModelSequenceItemList[currentIndex].normalMap);
        meshFilter.mesh = ModelSequenceItemList[currentIndex].mesh;
        //ModelSequenceItemList[currentIndex].obj.SetActive(true);
        //currentModelSequenceItem = ModelSequenceItemList[currentIndex];
    }

    [System.Serializable]
    public class ModelSequenceItem
    {
       // public GameObject obj;
        //public MeshRenderer meshRenderer;
        public Mesh mesh;
        public Texture baseMap;
        public Texture normalMap;
        public Texture maskMap;
    }

    ///// <summary>
    ///// Called when any error occurs.
    ///// </summary>
    ///// <param name="obj">The contextualized error, containing the original exception and the context passed to the method where the error was thrown.</param>
    //private void OnError(IContextualizedError obj)
    //{
    //    Debug.LogError($"An error occurred while loading your Model: {obj.GetInnerException()}");
    //}

    ///// <summary>
    ///// Called when the Model loading progress changes.
    ///// </summary>
    ///// <param name="assetLoaderContext">The context used to load the Model.</param>
    ///// <param name="progress">The loading progress.</param>
    //private void OnProgress(AssetLoaderContext assetLoaderContext, float progress)
    //{
    //    Debug.Log($"Loading Model. Progress: {progress:P}");
    //}

    ///// <summary>
    ///// Called when the Model (including Textures and Materials) has been fully loaded, or after any error occurs.
    ///// </summary>
    ///// <remarks>The loaded GameObject is available on the assetLoaderContext.RootGameObject field.</remarks>
    ///// <param name="assetLoaderContext">The context used to load the Model.</param>
    //private void OnMaterialsLoad(AssetLoaderContext assetLoaderContext)
    //{
    //    Debug.Log("Materials loaded. Model fully loaded.");
    //}

    ///// <summary>
    ///// Called when the Model Meshes and hierarchy are loaded.
    ///// </summary>
    ///// <remarks>The loaded GameObject is available on the assetLoaderContext.RootGameObject field.</remarks>
    ///// <param name="assetLoaderContext">The context used to load the Model.</param>
    //private void OnLoad(AssetLoaderContext assetLoaderContext)
    //{
    //    assetLoaderContext.RootGameObject.transform.eulerAngles = new Vector3(180, 0, 0);
    //    assetLoaderContext.RootGameObject.transform.position = modelPos.transform.position;
    //    assetLoaderContext.RootGameObject.transform.parent = modelParent;
    //    Debug.Log("Model loaded. Loading materials.");
    //}

    public bool IsFileExist(string path)
    {
        return File.Exists(path);
    }

    IEnumerator DoWebRequestGetTexture(string url, Action<Texture> onSuccess, Action<string> onError = null)
    {
        using (UnityWebRequest unityWebRequest = UnityWebRequestTexture.GetTexture(url))
        {
            yield return unityWebRequest.SendWebRequest();

            if (unityWebRequest.isNetworkError || unityWebRequest.isHttpError)
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
        currentIndex += 1;
        if (currentIndex > ModelSequenceItemList.Count - 1)
        {
            currentIndex = 0;
        }
        UpdateModel(currentIndex);       
    }

    public void Last()
    {
        currentIndex -= 1;
        if (currentIndex <0)
        {
            currentIndex = ModelSequenceItemList.Count - 1;
        }
        UpdateModel(currentIndex);       
    }
}
