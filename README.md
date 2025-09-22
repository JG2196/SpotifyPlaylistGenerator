
# SpotifyPlaylistGenerator

## 🎵Introduction
SpotifyPlaylistGenerator a Blazor Server web application that allows users to authenticate with Spotify, view and manage their playlists, and generate new playlists using AI. Users can log in, browse their existing playlists, and use OpenAI-powered prompts to create custom playlists based on their mood or preferences creating a modern music experience.

## ⭐Features

- **Spotify authentication and playlist management** - Display the signed in users playlists, along with information such as: playlist artwork, tracks / track information, playlist description, play time and link direct to playlist.
- **AI-powered playlist generation using OpenAI** - Using OpenAI, users can input prompts which will return a list of available tracks in Spotify. Playlists can then be created and added to the users account.
- **Interactive playlist creation and modification** - Select the tracks to be added to your playlist based on the AI generation.
- **Simple model alteration** - Easily alter which OpenAI model to use for prompt submission.
- **Robust Spotify API handling** - Concise API requests to avoid reaching Spotify's rate limit.


## Tech Stack

**Framework:** .Net Blazor

**Client:** JavaScript, TailwindCSS

**API:** OpenAI, Spotify Web

**Tunneling:** NGROK (optional)
## Installation

Clone my project via github or

```
  git clone https://github.com/JG2196/BlazorApp1.git
  cd BlazorApp1
```

### Prerequisites

- Spotify Developer account
- Open AI account
- Web tunnel (optional)

### Quick Start

#### Spotify Application

Create a Spotify Application from the [Spotify Developer Dashboard](https://developer.spotify.com/).

Set the redirect URI path with a path of `/signinauth`, example `'https://playlistgenerator.com/signinauth'`.
> [!IMPORTANT]
> Spotify's security update no longer supports localhost as a redirect URI. To get around this use a tunneling service like (NGROK) whilst in development, or an alternative.

Select Web API as the API that the application is using. Continue to create the application.

#### OpenAI Application

Create a new project.

Create a secret key for the application. Make sure to take note of the secret key provided.

Add credits to your application.

#### Configure appsettings.json

Set the secrets, keys, ids and redirect URI.

For the OpenAI Model, select the model that will best fit your needs.

```
{
  "SpotifyWeb": {
    "ClientId": "your_spotify_client_id",
    "ClientSecret": "your_spotify_client_secret",
    "AuthAddress": "https://accounts.spotify.com/authorize",
    "RedirectUri": "your_redirect_uri",
    "Scopes": "your_required_scopes",
    "SignOutUrl": "your_signout_url"
  }
  "OpenAI": {
    "APIKey": "your_openai_api_key"
  },
}
```

#### Run Application
Before running the application restore the npm packages.

## Demo

**Display the signed in users playlists and tracks in a selected playlist**

![](/assets/PlayGen_Playlists.png)

**Submit a prompt to generate a playlist**

![](/assets/PlayGen_SubmitPrompt.png)

**Displays the results of the prompt, allowing the user to select which tracks to include in the playlist**

![](/assets/PlayGen_AIResult.png)

**The newly created playlist added to the users account**

![](/assets/PlayGen_CreatedPlaylist.png)

## 🚀Future Improvements

- **AI Integration Optimization:** Implement modular AI agents tailored to the project, minimizing resource and improving scalability.
- **Enhanced AI Response Accuracy:** Refine response logic to generate playlists that more closely align with user prompts.
-	**Track Playback Preview:** Enable users to preview individual tracks.
- **Playlist Editing:** Introduce functionality for users to modify existing playlists, including renaming, adding tracks and removing tracks.

