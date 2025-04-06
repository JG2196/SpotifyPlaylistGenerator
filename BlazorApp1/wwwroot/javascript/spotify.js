function spotifyDisplayPlaylists(jsonListPlaylists) {
    
    let obj = JSON.parse(jsonListPlaylists);

    let elm_img, elm_div, elm_h, elm_b;

    const spotifyPlaylistsDisplay = document.getElementById("spotifyPlaylistsDisplay");

    const arrClasses = ["w3-card", "w3-bar-item", "w3-round-xlarge"];

    for (index in obj) {

        elm_div = document.createElement("div");

        for (let i = 0; i < arrClasses.length; i++) {
            elm_div.classList.add(arrClasses[i]);
        }

        elm_div.style.paddingTop = "16px";
        elm_div.style.margin = "8px";

        elm_img = document.createElement("img");
        elm_img.classList.add("w3-round-large");
        if (obj[index].Images != null) {
            elm_img.src = obj[index].Images[0].Url;
        }
        elm_img.style.width = "200px"
        elm_img.style.height = "200px"
        elm_div.appendChild(elm_img);

        elm_h = document.createElement("h5");
        elm_h.style.textAlign = "right";

        elm_b = document.createElement("b");
        elm_b.innerText = spotifyAlterString(obj[index].Name);
        elm_h.appendChild(elm_b);
        elm_div.appendChild(elm_h);

        spotifyPlaylistsDisplay.appendChild(elm_div);
    }
}
function spotifyAlterString(str) {

    const strLength = str.length;
    const strSpacer = "...";

    if (strLength > 20) {
        str = str.substring(0, 17) + strSpacer;
    }

    return str;
}