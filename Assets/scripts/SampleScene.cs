using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DefaultNamespace;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yohkan.runtime.scripts;
using yohkan.runtime.scripts.interfaces;
using Cysharp.Threading.Tasks;

public class SampleScene : MonoBehaviour, IAssetResolveEvent
{
   [SerializeField] private Button _initializeButton = null;
   [SerializeField] private Button _reserveButton = null;
   [SerializeField] private Button _resolveButton = null;
   [SerializeField] private Button _displayButton = null;
   [SerializeField] private Image[] _images = null;
   [SerializeField] private TextMeshProUGUI _text = null;

   private YohkanAssetProvider _provider = null;
   private async void Awake()
   {
       _initializeButton.onClick.AddListener(async () =>
       {
           await AssetBundleManagerHolder.Manager.InitializeAsync(CancellationToken.None);
           _provider = AssetBundleManagerHolder.Manager.CreateAssetBundleProvider(this);
           _text.text = "Initialize Finish!";
       });
       
       _reserveButton.onClick.AddListener(() =>
       {
           var reserver = _provider as IAssetReserver;
           reserver.ReserveAsset<Sprite>("dr01");
           reserver.ReserveAsset<Sprite>("dr02");
           reserver.ReserveAsset<Sprite>("dr03");
           _text.text = "resource reserved!";
       });
       
       _resolveButton.onClick.AddListener(async () =>
       {
           var resolver = _provider as IAssetResolver;
           await resolver.ResolveAsync(CancellationToken.None);
           _text.text = "Download / Load Finish!";
       });
       
       _displayButton.onClick.AddListener(() =>
       {
           var container = _provider as IAssetContainer;
           _images[0].sprite = container.GetAsset<Sprite>("dr01");
           _images[1].sprite = container.GetAsset<Sprite>("dr02");
           _images[2].sprite = container.GetAsset<Sprite>("dr03");
       });
       
       
   }
   
   async UniTask<bool> IAssetResolveEvent.AskDownloadConfirm(long downloadFileSizeByte)
   {
       _text.text = $"DownloadSize: {downloadFileSizeByte}byte.";
       await Task.Delay(TimeSpan.FromSeconds(1));
       return true;
   }

   void IAssetResolveEvent.OnUpdateDownloadProgress(float normalizedProgress)
   {
       _text.text = $"Downloading: {(int)(normalizedProgress * 100)}%";
   }
}
