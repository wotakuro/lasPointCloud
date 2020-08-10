# lasPointCloud

lasファイルを点群として読みこんでUnity上で扱うプロジェクトです。<br />
静岡県が公開している点群データを読みこみたくて始めました。<br />
https://pointcloud.pref.shizuoka.jp/

## 現在確認できているフォーマット
<pre>
・白糸の滝
 工事番号	28-XXX00-03-00-04 案件名称	白糸の滝滝見橋周辺整備事業　その４
　V1.3 format3
 </
 
## 使用方法下記のような形です。
<pre>
int reductionParam=1;// リダクションするパラメーター。リダクションは単純に点の情報を何も考えず間引いているだけです。
PointCloud.LasFormat.LasLoader.Instantiate(lasのファイルパス, material, reductionParam);


※Materialに割り当てるシェーダーは、Unlit/PointCloudシェーダーをお使い下さい。
</pre>
