# Web app and auth

## Build Status

[![Build Status](https://dev.azure.com/jannemattila/jannemattila/_apis/build/status/JanneMattila.326-webapp-and-folders?branchName=master)](https://dev.azure.com/jannemattila/jannemattila/_build/latest?definitionId=45&branchName=master)

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Compare OpenID Connect endpoints

https://login.microsoftonline.com/<tenantid>/.well-known/openid-configuration

```json
"userinfo_endpoint" : "https://login.microsoftonline.com/<tenantid>/openid/userinfo",
```

https://login.microsoftonline.com/<tenantid>/v2.0/.well-known/openid-configuration

```json
"userinfo_endpoint" : "https://graph.microsoft.com/oidc/userinfo",
```