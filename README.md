# NGM.OpenAuthentication
Provides multiple authentication features for Orchard CMS using OpenId, OAuth, OAuth2, and AzureAD

Currently requires you to add an assembly redirect to the root Orchard web.config file:
```xml
<dependentAssembly>
   <assemblyIdentity name="DotNetOpenAuth.AspNet" publicKeyToken="2780ccd10d57b246" culture="neutral" />
	 <bindingRedirect oldVersion="0.0.0.0-4.3.0.0" newVersion="4.3.0.0" />
</dependentAssembly>
```

I'm working on finding a cleaner solution
