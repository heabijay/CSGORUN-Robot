# CSGORUN-Robot

The system which aggregates information from multiple chats, performs promo-code search and activates it on [csgorun.pro](https://csgorun.pro) instead of you in many accounts using many proxies! 

**This project is currently unsupported since August 2021. And it was in private access until May 2022. All functionality haven't been tested since 2021.**


## Features

- Add multiple CSGORUN accounts.
- Add multiple HTTP/SOCKS5 proxies to use with target account.
- Add multiple chat to aggregate (CSGORUN en/ru chats, Twitch streamers chats, Telegram channels feed) or easily code your own service connector.
- Automatically detect promo-codes in message using regular expressions or manually push promo to queue.
- Automatically enqueue promo-code for each account list and perform activation on it.
- Automatically place bet on custom-game (crash, roulette, etc) after success promo activation.
- Change custom delays random range. 
- Calculate deposit/withdraw statistic using csgorun data and simple calculation.
- Control own service using telegram-bot and receive logs to file and critical logs right to telegram. Telegram bot commands: 

```
/accounts — Provides latest information about accounts.
/debugactivatepromo — Debug option. Performs activate promo at all accounts.
/debugplacebet — Debug option. Places default bet.
/debugrefreshcurrentstate — Debug option. Performs current state refresh on selected account.
/help — Provides the help information with available commands.
/settoken — Sets a new token to selected account.
/setuseragent — Sets a new user-agent to selected account.
/shutdown — Performs shutdown of the CSGORUN-Robot.
/start — Bot's welcome message!
/tgaggregator — Directs messages to Telegram Aggregator Account IO.
/withdraws — Calculates deposit / withdraws statistic.
```


## Settings

<details>
<summary>Example of settings.json</summary>

```json
{
  "CSGORUN": {
    "Accounts": [
      {
        "AuthToken": "eyJhbGciOiJIUzI1NiIsInAccount.With.HTTPProxy",
        "UserAgent": "WARNING!!! NEXT USER-AGENT IS DEFAULT FOR NULL VALUE - Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.163 Safari/537.36",
        "Proxy": {
          "Type": "HTTP",
          "Host": "Proxy.host.com",
          "Port": 80,
          "Username": "user",
          "Password": "pwd"
        }
      },
      {
        "AuthToken": "eyJhbGciOiJIUzI1NiIsInAccount.With.SOCKS5Proxy",
        "UserAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:84.0) Gecko/20100101 Firefox/84.0",
        "Proxy": {
          "Type": "SOCKS5",
          "Host": "Proxy.host.com",
          "Port": 80,
          "Username": "user",
          "Password": "pwd"
        }
      },
      {
        "AuthToken": "eyJhbGciOiJIUzI1NiIsInAccount.Without.Proxy",
        "UserAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.163 Safari/537.36",
        "Proxy": null
      }
    ],
    "RegexPatterns": {
      "Default": "Pattern Default. Dont forget about bypass quatation mark using escape char.",
      "EN_Admins": "Pattern for english chat admins. Dont forget about bypass quatation mark using escape char.",
      "RU_Admins": "Pattern for russian chat admins. Dont forget about bypass quatation mark using escape char."
    },
    "AutoPlaceBet": false,
    "RequestsDelay": 5500,
    "BeforeActivationDelay": {
      "Min": 1500,
      "Max": 3000
    },
    "PlaceBetDelayAfterGameStartDelay": {
      "Min": 1000,
      "Max": 3000
    },
    "PlaceBetSkipGames": {
      "Min": 0,
      "Max": 0
    },
    "PromoCache": {
      "Lifetime_Minutes": 30
    },
    "PromoExclusion": [
      "CSGORUN",
      "YOURUN"
    ]
  },
  "Twitch": {
    "Channels": "xQcOW, StRoGo",
    "RegexPattern": "Pattern for channel owner messages. Dont forget about bypass quatation mark using escape char."
  },
  "Telegram": {
    "Aggregator": {
      "ApiId": 0,
      "ApiHash": "Your telegram API Hash",
      "Channels": [
        {
          "Username": "runcsgo",
          "Regex": "Regex Pattern"
        },
        {
          "Username": "runcsgo2",
          "Regex": "Another Pattern"
        }
      ]
    },
    "Bot": {
      "BotToken": "1234567890:AAAaaAAaa_AaAAaa-AAaAAAaAAaAaAaAAAA",
      "OwnerId": 0
    }
  }
}
```
</details>

- **CSGORUN** - Section with csgorun.pro settings:
  - **Accounts** - Array with csgorun.pro accounts:
    - **AuthToken** - JWT token of csgorun.pro account. Can be accessed from authorized page using in console:

        ```js
        prompt("auth-token", localStorage['auth-token']);
        ```

    - **UserAgent** - User-agent to use with this account. By default "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.163 Safari/537.36".

    - **Proxy** - Http/Socks5 proxy to use with this account.
  - **RegexPatterns** - Regular expressions for promo-code search in csgorun own chat.
    - **Default** - Applies to all typical users messages in chat.
    - **EN_Admins** - Applies only to english-chat admins messages.
    - **RU_Admins** - Applies only to non-english-chat admins messages.
  - **AutoPlaceBet** - Is automatic place bet enabled. Due once csgorun just prohibited to activate promo without bet before.
  - **RequestsDelay** - Delay before periodical api delay requests.
  - **BeforeActivationDelay** (min/max) - Delay after received chat promo to activate.
  - **PlaceBetDelayAfterGameStartDelay** (min/max) - Delay site game started to place bet.
  - **PlaceBetSkipGames** (min/max) - Skip a few games after activate promo.
  - **PromoCache.Lifetime_Minutes** - By default, promo-codes has been cached in RAM dictionary to prevent wrong activate duplications.
  - **PromoExclusion** - By default, promo-codes has been cached in RAM dictionary to prevent wrong activate duplications.
- **Twitch** - Section with twitch chat integration settings:
  - **Channels** - Comma-separated twitch channel list to aggregate theres chats. Keep in mind there are aggregates only chat-owner own messages.
  - **RegexPattern** - Regular expression pattern.
- **Telegram** - Section with telegram bot and aggregator integration settings:
  - **Aggregator** - Section with telegram aggregator settings (telegram-client):
    - **ApiId**, **ApiHash** - application identifier from [my.telegram.org](https://my.telegram.org).
    - **Channels** - Array with telegram usernames with channels and regex patterns. Keep in mind theese channels will be joined from connected telegram account.
  -  **Bot** - Section with telegram bot settings (telegram-bot) to interact and control  this system by remote:
     -  **BotToken** - bot token from [@BotFather](https://t.me/BotFather).
     -  **OwnerId** - id of your telegram account. (because only you can use bot). [Can be received by some simple it bot (google it).]


## Screenshots

<details>
<summary>Click to show</summary>

![](https://user-images.githubusercontent.com/50156936/167207289-6277ac02-c5bc-4a14-8a49-cc2ec37e3d16.png)
![](https://user-images.githubusercontent.com/50156936/167208370-500323ec-a991-43c0-9135-63dc71c0c2d6.png)
![](https://user-images.githubusercontent.com/50156936/167208392-8edace22-ad25-43cb-ab11-8f2c8f645642.png)
![](https://user-images.githubusercontent.com/50156936/167208401-57c7613e-6123-4666-86eb-b66a3f4bffcc.png)

</details>