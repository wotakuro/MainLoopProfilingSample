# MainLoopProfilingSample
Read this in other languages: [English](README.md), 日本語<br />

## これについて
<img src="doc/img/Screenshot.jpg" />

Unity 2018.1から導入された PlayerLoopを利用して実機上での簡易プロファイリングツールです。<br />
画面左上のように、大まかな項目別に処理具合を実機で確認出来ます

## メーターについて
<img src="doc/img/explain.jpg" />

<pre>
1. MainThreadの処理時間メーター
　水色：Script
　オレンジ：Physics
　赤色：Animator
　緑色：Render
　紫：その他

2.RenderThreadの処理時間メーター

3.直近1秒のフレームレート情報
4.直近1秒の最大処理時間と最小処理時間

</pre>
