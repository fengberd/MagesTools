<?php
$output = array();

function xd($id)
{
    global $child;
    return preg_split('/\\\\r[se]/u', str_ireplace('\n', "\n", $child->string[$id]))[0];
}

$xml = simplexml_load_file('local/tips.xml');
foreach ($xml->children() as $child) {
    $child = $child->children();
    $output[] = array(
        'category' => xd(0),
        'content' => xd(1),
        'name' => xd(2),
        'extra' => xd(3),
        'sort_key' => xd(4),
    );
}
file_put_contents('local/tips_new.json', json_encode($output, JSON_PRETTY_PRINT | JSON_UNESCAPED_UNICODE));
