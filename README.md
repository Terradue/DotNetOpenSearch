# DotNetOpenSearch - OpenSearch library for .Net

Terradue.OpenSearch is a library targeting .NET 4.0 and above providing an easy way to perform OpenSearch query from a class or an URL to multiple and custom types of results (Atom, Rdf, etc.)

## Usage examples

Query 

```c#
// First create the engine
var engine = new OpenSearchEngine();
// Load the extensions automatically
engine.LoadPlugins();

// Create a generic OpenSearchable from an URL
var entity = new GenericOpenSearchable(new OpenSearchUrl("http://eo-virtual-archive4.esa.int/search/ASA_IM__0P/atom"), engine);
// Specify the requested parameters
var parameters = new NameValueCollection();
parameters.Add("count", "20");
parameters.Add("start", "1992-01-01");
parameters.Add("stop", "2014-04-15");
parameters.Add("bbox", "24,30,42,53");

// Query !
var result = engine.Query(entity, parameters, "Atom");

// Write the result
XmlWriter atomWriter = XmlWriter.Create("result.xml");
Atom10FeedFormatter atomFormatter = new Atom10FeedFormatter((SyndicationFeed)result.Result);
atomFormatter.WriteTo(atomWriter);
atomWriter.Close();
```

## Supported Platforms

* .NET 4.0 (Desktop / Server)
* Xamarin.iOS / Xamarin.Android / Xamarin.Mac
* Mono 2.10+

## Getting Started

Terradue.OpenSearch is available as NuGet package in releases.

```
Install-Package Terradue.OpenSearch
```

## Build

Terradue.OpenSearch is a single assembly designed to be easily deployed anywhere. 

To compile it yourself, you’ll need:

* Visual Studio 2012 or later, or Xamarin Studio

To clone it locally click the "Clone in Desktop" button above or run the 
following git commands.

```
git clone git@github.com:Terradue/Terradue.OpenSearch.git Terradue.OpenSearch
cd Terradue.OpenSearch
./configure
./make
```

## TODO

* Specialize The OpenSearchResult with an interface
* Wrap SyndicationFeed with a class implementing the OpenSearchResult Interface
* Testing!

## Copyright and License

Copyright (c) 2014 Terradue

Licensed under the [GPL v2 License](https://github.com/Terradue.GeoJson/Terradue.OpenSearch/blob/master/LICENSE.txt)

