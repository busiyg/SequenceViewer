using System.Collections;
using System.Collections.Generic;
using System.IO;
using TriLibCore;
using UnityEngine;

public class MainManager : MonoBehaviour
{
    public ModelSequenceItem currentModelSequenceItem;
    public List<ModelSequenceItem> ModelSequenceItemList;
    public string filePath;
    public string folderPath;
    public List<GameObject> models;

    public bool isPlaying;
    public Material baseMaterial;
    public Transform modelPos;
    public Transform modelParent;
    public int maxSize;
    public int startIndex;
    public int currentIndex;
    public float interval;
    public AssetLoaderOptions assetLoaderOptions;
    // Start is called before the first frame update
    void Start()
    {
        // ReadFiles
        assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions();
        ModelSequenceItemList = new List<ModelSequenceItem>();
        //var obj = AssetLoader.LoadModelFromFileNoThread(filePath);
        // var obj = AssetLoader.LoadModelFromFileNoThread()
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ReadFiles()
    {
        Debug.LogWarning("ReadFiles");
        for (int i = startIndex; i < startIndex+maxSize; i++)
        {

            string modelFilePath = $"{folderPath}0000{i}.obj";
            Debug.LogWarning(modelFilePath);
            bool bo = IsFileExist(modelFilePath);
            if (bo) {
                var obj = AssetLoader.LoadModelFromFileNoThread(modelFilePath);
                
                obj.RootGameObject.transform.eulerAngles = new Vector3(180, 0, 0);
                obj.RootGameObject.transform.position = modelPos.transform.position;
                obj.RootGameObject.transform.parent = modelParent;
                ModelSequenceItem modelSequenceItem = new ModelSequenceItem();
                modelSequenceItem.obj = obj.RootGameObject.transform.GetChild(0).gameObject;
                modelSequenceItem.meshRenderer = modelSequenceItem.obj.GetComponent<MeshRenderer>();
                modelSequenceItem.meshRenderer.material = baseMaterial;
                modelSequenceItem.obj.SetActive(false);
                ModelSequenceItemList.Add(modelSequenceItem);

                //var obj = AssetLoader.LoadModelFromFile(modelFilePath, OnLoad, OnMaterialsLoad, OnProgress, OnError, null, assetLoaderOptions);
            }
            else {
                Debug.LogError("file not exsit");
            }
        }
        currentIndex = 0;
    }

    public void PlayOrPause()
    {
        if (isPlaying)
        {
            isPlaying = !isPlaying;
            Pause();
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
            if (currentModelSequenceItem!=null) {
                currentModelSequenceItem.obj.SetActive(false);
            }


            if (currentIndex> ModelSequenceItemList.Count-1) {
                currentIndex = 0;
            }

            ModelSequenceItemList[currentIndex].obj.SetActive(true);
            currentModelSequenceItem = ModelSequenceItemList[currentIndex];
            currentIndex += 1;


            yield return new WaitForSeconds(interval);
        }
    }

    public void Pause()
    {

    }

    public enum frameRate
    {
        fps_15,
        fps_30,
        fps_60
    }

    public class ModelSequenceItem
    {
        public GameObject obj;
        public MeshRenderer meshRenderer;
        public Texture baseTex;
        public Texture normalTex;
        public Texture mapTex;
    }

    /// <summary>
    /// Called when any error occurs.
    /// </summary>
    /// <param name="obj">The contextualized error, containing the original exception and the context passed to the method where the error was thrown.</param>
    private void OnError(IContextualizedError obj)
    {
        Debug.LogError($"An error occurred while loading your Model: {obj.GetInnerException()}");
    }

    /// <summary>
    /// Called when the Model loading progress changes.
    /// </summary>
    /// <param name="assetLoaderContext">The context used to load the Model.</param>
    /// <param name="progress">The loading progress.</param>
    private void OnProgress(AssetLoaderContext assetLoaderContext, float progress)
    {
        Debug.Log($"Loading Model. Progress: {progress:P}");
    }

    /// <summary>
    /// Called when the Model (including Textures and Materials) has been fully loaded, or after any error occurs.
    /// </summary>
    /// <remarks>The loaded GameObject is available on the assetLoaderContext.RootGameObject field.</remarks>
    /// <param name="assetLoaderContext">The context used to load the Model.</param>
    private void OnMaterialsLoad(AssetLoaderContext assetLoaderContext)
    {
        Debug.Log("Materials loaded. Model fully loaded.");
    }

    /// <summary>
    /// Called when the Model Meshes and hierarchy are loaded.
    /// </summary>
    /// <remarks>The loaded GameObject is available on the assetLoaderContext.RootGameObject field.</remarks>
    /// <param name="assetLoaderContext">The context used to load the Model.</param>
    private void OnLoad(AssetLoaderContext assetLoaderContext)
    {
        assetLoaderContext.RootGameObject.transform.eulerAngles = new Vector3(180, 0, 0);
        assetLoaderContext.RootGameObject.transform.position = modelPos.transform.position;
        assetLoaderContext.RootGameObject.transform.parent = modelParent;
        Debug.Log("Model loaded. Loading materials.");
    }

    public bool IsFileExist(string path)
    {
        return File.Exists(path);
    }
}
