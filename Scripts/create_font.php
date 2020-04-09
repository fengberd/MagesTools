<?php
function mb_str_split($str)
{
    return preg_split('//u', $str, -1, PREG_SPLIT_NO_EMPTY);
}

define('PATCH_FILE', $argv[1] ?? 'local/script.json');
define('DEFAULT_FONT', $argv[2] ?? realpath('C:/Windows/Fonts/DENGB.TTF'));
define('FALLBACK_FONT', $argv[3] ?? 'C:/Windows/Fonts/DENGB.TTF');

$patch = json_decode(file_get_contents(PATCH_FILE), true);

$charset = array();
$charset_preset = mb_str_split($patch['charset_preset']);

foreach ($patch['data'] as $val) {
    foreach ($val as $text) {
        if ($text == null) {
            continue;
        }
        foreach ($text as $segment) {
            foreach (mb_str_split($segment) as $c) {
                if ($c == "\r" || $c == "\n") {
                    continue;
                }
                if (!in_array($c, $charset_preset)) {
                    $charset[$c] = true;
                }
            }
        }
    }
}
$charset = array_keys($charset);
sort($charset);

$patch['charset'] = implode('', $charset);
file_put_contents(PATCH_FILE, json_encode($patch, JSON_PRETTY_PRINT | JSON_UNESCAPED_UNICODE));

$translation = json_decode(file_get_contents('translation.json'), true);
function createFont($output, $preset)
{
    global $charset, $translation;
    $img = ImageCreateTrueColor(3072, (ceil((count($charset) - 24) / 64) + 11) * 48);
    ImageSaveAlpha($img, true);
    ImageFill($img, 0, 0, ImageColorAllocateAlpha($img, 255, 255, 255, 127));

    $row = 10;
    $column = 24;

    $preset = ImageCreateFromPNG($preset);
    ImageCopy($img, $preset, 0, 0, 0, 0, 3072, ($row + 1) * 48);
    ImageDestroy($preset);

    $color = ImageColorAllocateAlpha($img, 255, 255, 255, 0);
    foreach ($charset as $c) {
        $font = DEFAULT_FONT;
        $box = ImageTTFBBox(36, 0, $font, $c);
        if ($box[0] == $box[3] && $box[3] == 0) {
            if (($pos = mb_strpos($translation['simplified'], $c)) !== false) {
                $box = ImageTTFBBox(36, 0, $font, $alias = mb_substr($translation['traditional'], $pos, 1));
                if ($box[0] != $box[3] || $box[3] != 0) {
                    $c = $alias;
                    goto DRAW;
                }
            }
            $font = FALLBACK_FONT;
        }
    DRAW: ImageTTFText($img, 36, 0, $column * 48, $row * 48 + 38, $color, $font, $c);
        if ($column++ >= 63) {
            $row++;
            $column = 0;
        }
    }
    ImagePNG($img, 'local/' . $output . '.png');
    ImageDestroy($img);

    @mkdir('local/system');
    system('cd local && texconv -y -f BC3_UNORM -nologo -ft DDS -o system ' . $output . '.png');
}

createFont('FONT', 'preset.png');
createFont('FONT2', 'preset2.png');
