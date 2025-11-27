What problems did I encounter?

**The DRY thing:**
The developer's greatest ally becomes their greatest enemy when it comes to microservices. As a software principle, “don't repeat yourself” aims to consolidate code we'll reuse repeatedly and eliminate code redundancy, which is always very useful. I developed the framework based on this principle, and ended up with the following projects:

WF.Shared.Application
WF.Shared.Infrastructure
WF.Shared.Domain!!! (yes, domain)

In the end, I created a parallel monolithic application by placing all the shared code in one place. When I realized there was a problem somewhere, I removed all the shared projects and rewrote every piece of code under its own service. It was indeed a huge workload, but that's why AI tools exist, right?

**What to include in the saga:**
This was one of the most challenging decisions. Some opinions suggest that the saga should contain as little information as possible, with services fetching the necessary data themselves, while others advocate for a “fat saga” structure; that is, the saga should carry everything (provided there are no security vulnerabilities), sparing services from extra processing inside. I went back and forth between these two options, but ultimately decided on the fat saga approach. If you look at the code, when a transaction starts, I provide the wallet numbers, etc., within the saga. This way, I aimed to reduce the number of queries made by the fraud service, for example.

**UserId transfer issue in audit logs:**
UserId is not a value that you can access from everywhere in the project or transfer to every layer. That's why I had to struggle quite a bit when transferring this information to the audit logs. Of course, it wasn't transferred via the HTTP context as standard. Here, I decided to transfer it via MassTransit headers. If you do this and are transferring sensitive data, don't forget to encrypt it.

**Introducing Keycloak roles:**
When trying to implement RBAC in Keycloak, you need to write a converter to introduce the same roles on the .NET side, and most of this part is written manually. Also, since this is required in every service, this code must not be forgotten. It caused me a significant amount of time loss, especially in the final stages of the project. Therefore, as I mentioned in the packages section, I might consider using another identity product that supports NuGet packages.


Thank you for taking the time to read this far. Please feel free to use the code in the project or contribute to it.
See you again soon.