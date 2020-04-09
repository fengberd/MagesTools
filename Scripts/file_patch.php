<?php
define('TARGET_DIR', $argv[1] ?? 'local/system');
define('PATCH_FILE', $argv[1] ?? 'local/system.json');

$final_data = array(
    'type' => 'file',
    'file' => basename(TARGET_DIR) . '.mpk',
    'data' => array()
);

$dir = opendir(TARGET_DIR);
$data = &$final_data['data'];
while ($file = readdir($dir)) {
    if ($file != '.' && $file != '..') {
        $data[basename($file)] = base64_encode(gzencode(file_get_contents(TARGET_DIR . '/' . $file), 9));
    }
}
closedir($dir);

file_put_contents(PATCH_FILE, json_encode($final_data, JSON_PRETTY_PRINT | JSON_UNESCAPED_UNICODE));
