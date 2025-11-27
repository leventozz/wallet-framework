Hangi problemlerle karşılaştım?

DRY konusu:
Yazılımcının en büyük yardımcısı, konu mikroservis olunca en büyük düşmana dönüşüyor. Bir yazılım ilkesi olarak "don’t repeat yourself"in amacı tekrar tekrar kullanacağımız kodları ortaklaştırıp kod fazlalığından kurtulmaktır ki her zaman da çok işe yarar. Ben de bu düsturla framework’ü geliştirdim ve elimde şu projeler oldu:

WF.Shared.Application
WF.Shared.Infrastructure
WF.Shared.Domain!!! (evet domain)

Günün sonunda ortak olan kodları bir yere alarak paralel bir monolith uygulama kurdum. Bir yerde sorun olduğunu anladığımda bütün shared projeleri kaldırdım ve her kodu kendi servisi altında tekrar tekrar yazdım. Gerçekten de büyük bir iş yüküydü ama AI tool’lar neden var değil mi?

Saga’da neler taşınacağı konusu:
En çok ikilemde kaldığım konulardan birisi oldu bu. Kimi görüşler saga’nın olabildiğince az bilgi içermesini ve servislerin gerekli bilgileri kendi almasını önerirken, kimi görüşler “fat saga” yapısını savunuyor; yani saga ne varsa taşısın (güvenlik açığı olmayan), servisler içeride ekstra işle uğraşmasın. Ben de bu iki tercih arasında gidip geldim ama son olarak kararımı fat saga yönünde verdim. Kodu incelerseniz bir transaction başladığında wallet number’ları vs. saga’nın içinde veriyorum. Böylece örneğin fraud servisinin daha az sorgu yapmasını hedefledim.

Audit log’lara userId taşıma sorunu:
UserId projede her yerden erişebildiğiniz veya her katmana taşıyabildiğiniz bir değer değil. Bu yüzden audit loglarına bu bilgiyi taşırken baya uğraşmak zorunda kaldım. Standart olarak HTTP context üzerinden taşınmadı tabii ki. Burada da kararım MassTransit header’ları aracılığıyla taşımaktan yana oldu. Eğer bunu yapıyorsanız ve hassas data taşıyacaksanız encrypt etmeyi unutmayın.

Keycloak rollerinin tanıtılması:
Keycloak’ta RBAC yapmaya çalışırken aynı rollerin .NET tarafında tanıtılması için bir dönüştürücü yazmanız gerekiyor ve bu kısmın büyük bir bölümü manuel olarak yazılıyor. Aynı zamanda her serviste bu gerektiği için bu kodun unutulmaması gerekiyor. Özellikle projenin son safhalarında bana büyük zaman kaybı yaşattı. O yüzden paketler kısmında belirttiğim gibi NuGet paketi desteği olan başka bir identity ürünü kullanmayı düşünebilirim.


Buraya kadar vakit ayırıp okuduğunuz için teşekkür ederim. Lütfen projedeki kodları kullanmaktan veya projeye katkı yapmaktan çekinmeyin.
Tekrar görüşmek üzere.