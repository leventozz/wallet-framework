Why did I choose this package?

In this section, I want to explain why I chose this particular package. Also, since this part of the framework is the most open to debate, feel free to express your opinion.

**MassTransit:**
It was recently announced that it would be switching to a paid license. For this reason, I resisted using it for a while. However, it didn't make sense to ignore a product that has proven itself in the industry for so long just for this reason. I had worked on a finance project that used its biggest competitor, NServiceBus. I don't know if it was because of NServiceBus, but the structure seemed quite complex to me; I struggled for a long time to understand what was doing what.
MassTransit also directly helped solve a problem I encountered in the project. Passing the identity of the user performing the operation to lower layers for audit logs had become a serious challenge. The MassTransit middleware was already ready to solve this problem.

**YARP:**
I chose YARP for the gateway project in the microservice architecture. It was extremely practical to use. I know I wasn't aware of all of YARP's capabilities and didn't use all of them; I only focused on what I needed. I had previously used Ocelot as a gateway in a project, but it seemed quite complex. That's why I gave YARP a chance, and I was satisfied with the result.

**Keycloak:**
The lack of a license fee was a significant factor. It also performs quite well in distributed architectures. However, if I were to start the project again, I probably wouldn't choose it; because I couldn't find a reliable NuGet package for API usage and had to write all the processes myself. This was both time-consuming and created unnecessary complexity.

As for the other products I used, since most of you have likely encountered them at least once, I won't go into further detail.

Now, if you'd like, I can discuss the problems I encountered:
[Challenges](problems.md)

