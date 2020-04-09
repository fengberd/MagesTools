<?php
define('PATCH_FILE', $argv[1] ?? 'local/script.json');
define('RPY_LOCATION', $argv[2] ?? 'local/rpy');

$final_data = array(
    'type' => 'scx',
    'file' => 'script.mpk',
    'charset' => '',
    'charset_preset' => ' 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz　' .
        '/:-;!?\'.@#%~*&`()°^>+<ﾉ･=″$′,[\]_{|}…' .
        '０１２３４５６７８９ＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚ、。' .
        '，．：；？！゛゜‘’“”（）〔〕［］｛｝〈〉《》「」『』【】＜＞〖〗・⋯〜ー♪―ぁぃぅぇぉっゃゅょゎァィゥェォッャュョヮヵヶ①②' .
        '③④⑤⑥⑦⑧⑨⑩⑪⑫⑬ⁿ²％–—＿／•' .
        'βγζημξρστυφχψωÅ√◯⌐¬∣¯Д∥αδεθικλνο' .
        'πヽヾゝゞ〃仝々〆〇＼＋－±×÷＝≠＜＞≦≧∞∴♂♀℃￥＄￠￡％＃＆＊＠§☆★○●◎◇◆□■△▲▽▼※〒→←↑↓〓∈∋⊆⊇⊂⊃∪' .
        '∩∧∨￢⇒⇔∀∃∠⊥⌒∂∇≡≒≪≫∽∝∵∫∬‰♯♭♪†‡¶あいうえおかがきぎくぐけげこごさざしじすずせぜそぞただちぢつづてでとど' .
        'なにぬねのはばぱひびぴふぶぷへべぺほぼぽまみむめもやゆよらりるれろわゐゑをんアイウエオカガキギクグケゲコゴサザシジスズセゼソゾタ' .
        'ダチヂツヅテデトドナニヌネノハバパヒビピフブプヘベペホボポマミムメモヤユヨラリルレロヮワヰヱヲンヴΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟ' .
        'ΠΡΣΤΥΦΧΨΩⅠⅡⅢⅣⅤⅥⅦⅧⅨⅩ∮∑∟⊿Я',
    'data' => array()
);

$dir = opendir(RPY_LOCATION);
$scx = &$final_data['data'];
while ($file = readdir($dir)) {
    if (substr($file, -4) == '.rpy') {
        $data = array_map('trim', explode("\n", file_get_contents(RPY_LOCATION . '/' . $file)));
        $scx[$file = (basename($file, '.rpy') . '.SCX')] = array();
        foreach ($data as $val) {
            if (isset($val[0]) && $val[0] == '"') {
                $scx[$file][] = array_merge(...array_map(function ($v) {
                    return preg_split('/{(color=.+?|\/color)}/', $v);
                }, explode('" "', trim($val, '"'))));
            }
        }
    }
}
closedir($dir);

file_put_contents(PATCH_FILE, json_encode($final_data, JSON_PRETTY_PRINT | JSON_UNESCAPED_UNICODE));
