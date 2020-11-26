var replace = require("replace");
const fs = require('fs');

var key = fs.readFileSync('bing-maps-key', 'utf8');

fs.copyFile('src/index.html', 'wwwroot/index.html', (err) => {
    if (err) throw err;

    replace({
        regex: "bingmapskey",
        replacement: key,
        paths: ['wwwroot/index.html'],
        recursive: true,
        silent: true,
    });
});

