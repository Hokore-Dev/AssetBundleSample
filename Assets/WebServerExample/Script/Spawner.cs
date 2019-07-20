using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using System.Collections;

public enum eLoadType
{
    Resources,
    PackageAssetBundle,
    AmazonS3,
}

public enum eAssetType
{
    Plus,
    Minus,
}

public class Spawner : MonoBehaviour
{
    [SerializeField]
    private eLoadType _loadType;

    [SerializeField]
    private string amazon_s3_url;

	private const float SPAWN_TIME = 0.2f;
	private float _timer = 0;
	private bool _bSpwan = false;

    private GameObject _plusObject = null;
    private GameObject _minusObject = null;

	public void StartSpwan()
	{
        StartCoroutine(PreLoad(eAssetType.Plus, go =>
        {
            _plusObject = go;
            CheckLoadAllResource();
        }));
        StartCoroutine(PreLoad(eAssetType.Minus, go =>
        {
            _minusObject = go;
            CheckLoadAllResource();
        }));

		_bSpwan = true;
	}
	
	private void FixedUpdate()
	{
		if (!_bSpwan)
			return;
		
		_timer += Time.fixedDeltaTime;
		if (_timer >= SPAWN_TIME)
		{
			_timer = 0;
			CreateRandomObject();			
		}
	}

    private void CheckLoadAllResource()
    {
        if (_plusObject != null && _minusObject != null)
            _bSpwan = true;
    }

    private IEnumerator PreLoad(eAssetType type, System.Action<GameObject> callback)
    {
        GameObject go = null;
        if (_loadType == eLoadType.Resources)
        {
            go = Resources.Load<GameObject>(string.Format("Prefab/{0}", type.ToString()));
        }
        else if (_loadType == eLoadType.PackageAssetBundle)
        {
            var myLoadedAssetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, type.ToString().ToLower()));
            if (myLoadedAssetBundle == null)
            {
                Debug.LogError("asset bundle load failed!");
                yield break;
            }
            go = myLoadedAssetBundle.LoadAsset<GameObject>(type.ToString());
        }
        else if (_loadType == eLoadType.AmazonS3)
        {
            string uri = string.Format("{0}/{1}", amazon_s3_url, type.ToString().ToLower());

            UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(uri, 0);
            yield return request.SendWebRequest();
            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
            go = bundle.LoadAsset<GameObject>(type.ToString());
        }

        if (go == null)
        {
            Debug.LogError("resource is null!");
        }

        callback?.Invoke(go);
        yield break;
    }

	private void CreateRandomObject()
	{
		string prefab_path = string.Empty;
		int type = Random.Range(0, 3);

        GameObject baseResource = null;
        if (type <= 1)
		{
            baseResource = _plusObject;
		}
		else if (type == 2)
		{
            baseResource = _minusObject;
        }

		GameObject go = Instantiate(baseResource);
		go.transform.SetParent(this.transform);
		go.transform.localPosition = new Vector2(Random.Range(0, 1280) - 640, 450);
        go.name = prefab_path;
	}
}
