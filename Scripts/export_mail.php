<?php
$output = array();

function xd($id)
{
    global $output, $child;
    if (strval($child->string[$id]) != '') {
        $output[] = preg_split('/\\\\r[se]/u', str_ireplace('\n', "\n", $child->string[$id]));
    }
}

$xml = simplexml_load_file('local/mail.xml');
foreach ($xml->children() as $child) {
    $child = $child->children();
    xd(2);
    xd(1);
    xd(0);
}
file_put_contents('local/mail.json', json_encode($output, JSON_PRETTY_PRINT | JSON_UNESCAPED_UNICODE));
