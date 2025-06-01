function showPlaylists(bShowPlaylists) {
    if (!bShowPlaylists) {
        document.getElementById("spotifyPlaylistsDisplay").style.display = "none";
        document.getElementById("spotifyPlaylistDisplay").style.display = "none";
        document.getElementById("aiPrompts").style.display = "";
    }
    else {
        document.getElementById("spotifyPlaylistsDisplay").style.display = "";
        document.getElementById("spotifyPlaylistDisplay").style.display = "none";
        document.getElementById("aiPrompts").style.display = "none";
        document.getElementById("btn_CreatePlaylist").style.display = "none";
    }
}
function populatePlaylists(authUserData) {

    const obj = JSON.parse(authUserData);
    const spotifyPlaylistsDisplay = document.getElementById("spotifyPlaylistsDisplay");

    document.getElementById("user_id").innerText = obj.SpotifyAuthUser.DisplayName;

    if (obj.ListSpotifyPlaylists.Playlists.length == 0) {
        return;
    }

    for (let index in obj.ListSpotifyPlaylists.Playlists) {

        let elm_Div = document.createElement("div");
        let elm_Img = document.createElement("img");
        let elm_H5 = document.createElement("h5");
        let elm_B = document.createElement("b");

        elm_Div.classList.add("w3-card");
        elm_Div.classList.add("w3-bar-item");
        elm_Div.classList.add("w3-round-xlarge");
        elm_Div.setAttribute("onclick", `@OpenPlaylist(${obj.ListSpotifyPlaylists.Playlists[index].Id}, ${obj.ListSpotifyPlaylists.Playlists[index].Name})`);
        elm_Div.style.paddingTop = "16px";
        elm_Div.style.margin = "8px";
        elm_Div.style.cursor = "pointer";

        elm_Img.classList.add("w3-round-large");
        if (obj.ListSpotifyPlaylists.Playlists[index].Images != null) {
            elm_Img.src = obj.ListSpotifyPlaylists.Playlists[index].Images[0].Url;
        }
        elm_Img.style.width = "200px";
        elm_Img.style.height = "200px";

        elm_H5.style.textAlign = "right";
        elm_B.innerText = obj.ListSpotifyPlaylists.Playlists[index].Name;

        elm_H5.appendChild(elm_B);
        elm_Div.appendChild(elm_Img);
        elm_Div.appendChild(elm_H5);
        spotifyPlaylistsDisplay.appendChild(elm_Div);
    }
}
function spotifySetSearchDisplay(jsonDataUser, jsonListPlaylists) {

    spotifyDisplayUserInfo(jsonDataUser);
    spotifyDisplayPlaylists(jsonListPlaylists);

}
function spotifyDisplayUserInfo(jsonDataUser) {
    //console.debug("spotifyDisplayUserInfo");

    let obj = JSON.parse(jsonDataUser);

    document.getElementById("user_id").innerText = obj.SpotifyID;
}
function displayNavigation() {
    document.getElementById("div_Sidebar").style.display = "";
}
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
        elm_div.style.cursor = "pointer";
        elm_div.setAttribute("onclick", `spotifyOpenPlaylist('${obj[index].Id}', '${obj[index].Name}')`);

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
function spotifyOpenPlaylist(playlistName) {
    
    document.getElementById("spotifyPlaylistsDisplay").style.display = "none";
    document.getElementById("spotifyPlaylistDisplay").style.display = "";
    document.getElementById("playlistName").innerText = playlistName;

}
//function testAsyncTask() {
//    console.debug("testAsyncTask START!");
//    DotNet.invokeMethodAsync('BlazorApp1', 'TestTask');
//}
function spotifyAlterString(str) {

    const strLength = str.length;
    const strSpacer = "...";

    if (strLength > 20) {
        str = str.substring(0, 17) + strSpacer;
    }

    return str;
}
function spotifyProcessChatResponse(jsonData) {

    var aiGeneratedPlaylist = document.getElementById("ai_GeneratedPlaylist");
    var aiListPlaylistTracks = document.getElementById("ai_ListPlaylistTracks");
    document.getElementById("btn_CreatePlaylist").style.display = "";

    let obj = JSON.parse(jsonData);

    document.getElementById("input_Prompt").value = "";
    document.getElementById("ai_PlaylistDescription").innerText = obj.description;

    for (var index in obj.playlist) {

        var elm_Li = document.createElement("li");

        //elm_Li.setAttribute("spotify_id", obj.playlist[index].spotify_id);
        elm_Li.setAttribute("track_name", obj.playlist[index].title);
        elm_Li.setAttribute("track_artist", obj.playlist[index].artist);
        elm_Li.style.listStyleType = "none";

        elm_Li.innerText = `${obj.playlist[index].title} - ${obj.playlist[index].artist}`;

        aiListPlaylistTracks.appendChild(elm_Li);
    }

    document.getElementById("display_Loading").style.display = "none";
    aiGeneratedPlaylist.style.display = "";
    document.getElementById("spotifyPlaylistDisplay").style.display = "none";
    document.getElementById("spotifyPlaylistsDisplay").style.display = "none";
}

function showLoading() {
    document.getElementById("display_Loading").style.display = "";
    document.getElementById("spotifyPlaylistDisplay").style.display = "none";
    document.getElementById("spotifyPlaylistsDisplay").style.display = "none";
}

function getTracks() {

    var listTracks = [];

    var aiListPlaylistTracks = document.getElementById("ai_ListPlaylistTracks");
    var child_Li = aiListPlaylistTracks.getElementsByTagName("li");

    for (var i = 0; i < child_Li.length; i++) {
        var spotifyTrackName = child_Li[i].getAttribute("track_name");
        var spotifyTrackArtist = child_Li[i].getAttribute("track_artist");

        var CreateTrack = {
            artist: spotifyTrackArtist,
            name: spotifyTrackName
        }
        listTracks.push(CreateTrack);
    }

    return JSON.stringify(listTracks);
}
function spotTest(displayName) {
    document.getElementById("user_id").innerText = displayName;
}
