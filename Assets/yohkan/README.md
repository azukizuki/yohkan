# Yohkan

YohkanはシンプルなAddressableAssetSystemのWrapperライブラリです
Yohkanを使うと以下の事がシンプルに実装できます

- リソースの事前予約
  - 必要なリソースを事前に予約し、まとめてDLを行う事が可能です
  - DL後のリソースはIAssetContainer経由で同期メソッドで読み込みが可能となります
- シンプルなビルドバージョン管理の提供
  - アセットバンドル及びcontentStateは指定したディレクトリに出力され、各プロジェクトでのバージョン管理にすぐに回せます

基本思想としては「Addressableがやってくれるところはそのままやらせつつ、運用を考慮したユースケースに対応できるラッパーを提供する」という思想です。


# 機能紹介

## Runtime
### YohkanAssetBundleManager
- 本ライブラリにおけるメインクラス
- カタログの更新、リソースの一括Download、ラベル指定DL、及び後述するYohkanAssetProviderのファクトリメソッドを提供する。

### YohkanAssetProvider
- 主に利用するクラスである
- IAssetReserver , IAssetResolver , IAssetContainer , IDisposableを継承している
- IAssetReserverを用いてリソースの予約を行う（アドレス・AssetReference両方指定可能）
- IAssetResolverを用いて予約したリソースの解決（必要に応じてダウンロード）を行う。このタイミングで予約したリソースはメモリに乗る
  - リソースのダウンロードが発生する際、ダウンロードを行うかの確認をユーザーに問うためのイベントや、ダウンロード進捗を通知するイベントをIAssetResolveEventインターフェースを経由しコールする。コンストラクタでIAssetResolveEventを渡すことでプロダクトの性質に応じたDL許可UIなどを提供できる
- IAssetContainerを用いてリソースの取得を行える（View側で呼ぶイメージ）この関数は同期であり、非同期処理を考慮せずに即リソースを取得できる
- IDisposableのDisposeをコールすることでAddressableのReleaseAPIをコールし、Containerで確保しているリソースを全て開放する。適切にIDisposable.Dispose()をコールさえすればメモリリークは発生しない

## Editor
TBD
