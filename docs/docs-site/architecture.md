**About the architecture I chose:**

Here, I will try to explain why I chose these architectures from my own perspective.

**Microservices:**
First, I want to start with the microservices architecture. Actually, I wasn't very sure when choosing this architecture; because even though it has been implemented for years, I still think it is an approach with too many gray areas. However, one of the most common problems I encountered in my experience was high traffic. Applications that perform financial transactions inevitably have high traffic, and scalability was critical to handle this traffic. I also researched the modular monolith structure as an alternative to microservices, and at one point, I even decided to proceed with this structure. However, independent scalability and other operational advantages prevailed, so I developed the framework using the microservices architecture.

**EDD and CQRS:**
These topics became a necessity at some point after the microservice architecture came into play. When working with microservices, different patterns would only complicate things. I have always enjoyed using CQRS in particular. Since its advantages are already well-known, I don't feel the need to explain them again.

**Saga pattern:**
The reason I use this pattern is because I encountered a wallet application in my career that was developed without using the saga pattern. Money transfers were written atomically to the database, and then a background job that constantly updated the balance was running via Hangfire. Yes, it was working; however, as the project progressed, this structure became unsustainable.

**DDD:**
It was a very popular approach early in my career. Even back then, I constantly heard people say, “DDD can't be fully implemented in projects.” I saw that this observation was correct as I gained experience working on projects that claimed to be DDD. Of course, it's not a rule that everyone must follow, but I experienced that when it wasn't applied properly, unit test writing also weakened to the same extent. In most projects, at most integration tests could be written (and often even those weren't written). When you wanted to write unit tests, you were only faced with private methods loaded with business logic. That's why I wanted to apply DDD as correctly as I could research it, and while writing unit tests, I realized how appropriate this choice was.

There are other architectural decisions I could mention in the project, but I think that would go into too much detail. Maybe I'll add to it later; for now, I'll leave it here.

Finally, I want to discuss why I chose certain packages:
[Packages](packages.md)
