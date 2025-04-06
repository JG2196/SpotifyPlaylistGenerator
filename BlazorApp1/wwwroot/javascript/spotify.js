function spotifyDisplayPlaylists(jsonListPlaylists) {
    console.debug("spotifyDisplayPlaylists: START");
    console.debug("spotifyDisplayPlaylists jsonlistPlaylists: " + jsonListPlaylists);

    let obj = JSON.parse(jsonListPlaylists);

    let elm_img;

    const spotifyPlaylistsDisplay = document.getElementById("spotifyPlaylistsDisplay");

    

    for (index in obj) {

        elm_img = document.createElement("img");
        elm_img.classList.add("w3-bar-item");
        if (obj[index].Images != null) {
            elm_img.src = obj[index].Images[0].Url;
        }
        elm_img.style.width = "200px"
        elm_img.style.height = "200px"
        spotifyPlaylistsDisplay.appendChild(elm_img);
        
    }
}