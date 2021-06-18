ReftabDotNet
=============

This is a quick and dirty module to interact with the Reftab API via .NET. Currently it returns json in string form only. You can convert to objects with whatever library you want.

# Instructions

```csharp
# Add the Reftab.cs file to your repository and include this line on any files you want to use the library.
using Reftab;
```

### Prerequisites

* .NET 5 or later
* A valid API key pair from Reftab
  * Generate one in Reftab Settings
  
# Examples

### Get an Asset and Update It

```csharp
#This example shows how to get an asset and update it

public async void Test() {
    //Create the ReftabApi connector setting the public and secret keys.
    Api ReftabApi = new Reftab.Api(<publicKey>, <secretKey>);
    try
    {
        //Get asset USNY013 from Reftab
        string asset = await ReftabApi.Request("GET", "assets/USNY013", "");
        
        //Replace the old title with the new title
        asset = asset.Replace("old title", "new title");
        
        //Put the changed json back to Reftab to update the asset
        string putResponse = await ReftabApi.Request("PUT", "assets/USNY013", asset);
    }
    catch (Exception e)
    {
        //If the API returns an unsuccessful HTTP code, ReftabApi will throw an exception with more information.
        System.Diagnostics.Debug.WriteLine(e.Message);
    }
}
```