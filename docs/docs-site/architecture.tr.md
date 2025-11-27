**Seçtiğim mimari hakkında:**

Burada seçtiğim mimarileri neden tercih ettiğimi kendi bakış açımdan açıklamaya çalışacağım.

**Mikroservisler:**
İlk olarak mikroservis mimarisinden başlamak istiyorum. Aslında bu mimariyi seçerken çok emin değildim; çünkü yıllardır uygulanıyor olsa da hâlâ gri alanı fazla olan bir yaklaşım olduğunu düşünüyorum. Ancak deneyimlerimde en sık karşılaştığım sorunlardan biri yüksek trafikti. Finansal işlemlerin yürütüldüğü uygulamalarda trafik kesin olarak yüksek oluyor ve bu trafiği karşılamak için ölçeklenebilirlik kritik önem taşıyordu. Mikroservisi seçmemek için modüler monolith yapısını da araştırdım, hatta bir ara bu yapıyla ilerlemeye karar vermiştim. Ancak bağımsız ölçeklenebilirlik ve diğer operasyonel avantajlar ağır bastığı için framework’ü mikroservis mimarisiyle geliştirdim.

**EDD ve CQRS:**
Bu konular mikroservis mimarisi devreye girdikten sonra bir noktada zorunluluk gibi karşıma çıktı. Mikroservislerle çalışırken farklı pattern’ler işleri yalnızca karmaşıklaştıracaktı. Özellikle CQRS kullanmaktan her zaman keyif almışımdır. Avantajları zaten bilinen noktalar olduğu için tekrar açıklama gereği duymuyorum.

**Saga pattern:**
Bu pattern’i kullanmamın sebebi kariyerimde denk geldiğim, saga pattern kullanılmadan geliştirilmiş bir cüzdan uygulaması oldu. Para transferleri atomik olarak veritabanına yazılıyor, ardından Hangfire aracılığıyla sürekli bakiye güncelleyen bir background job çalışıyordu. Evet, çalışıyordu; ancak proje ilerledikçe bu yapının ne kadar fazla açığı olduğunu gördüm. Günün sonunda sistem sürekli artan teknik borca dönüyordu.
Saga pattern’in zorluğu ise yeni yazılımcılar için akışın kavranmasının kolay olmaması. Yapıyı ilk kurduğum dönemlerde hangi event’ten sonra ne olacağını, hot path’in nasıl işleyeceğini anlamaya çalışırken zorlandım. Fakat buna rağmen MassTransit ve saga pattern birleştiğinde süreçlerin ne kadar kolay çözüldüğünü fark ettim. Bu ikili, karmaşık akışları üstlenme konusunda oldukça başarılı.

**DDD:**
Kariyerimin başlarında oldukça popüler olan bir yaklaşımdı. O zamanlar bile sürekli “DDD projelerde tam uygulanamıyor” denildiğini duyuyordum. Bu tespitin doğru olduğunu ddd olduğunu iddia eden projelerde çalışma fırsatı buldukça gördüm. Herkesin uygulaması gereken bir kural değil elbette, ancak düzgün uygulanmadığında uygulanmadığında unit test yazımının da aynı oranda zayıfladığını deneyimledim. Çoğu projede en fazla entegrasyon testleri yazılabiliyordu (ki çoğu zaman onlar bile yazılmıyordu). Unit test yazmak istediğinizde ise karşınıza yalnızca iş mantığı yüklü private metotlar çıkıyordu. Bu nedenle DDD’yi araştırabildiğim kadar doğru biçimde uygulamak istedim ve unit test yazarken bunun ne kadar yerinde bir tercih olduğunu anladım.

Projede bahsedebileceğim başka mimari kararlar da var ancak bunların daha fazla detaya gireceğini düşünüyorum. Belki ileride eklemelerde bulunurum; şimdilik burada bırakıyorum.

Son olarak hangi paketleri neden tercih ettiğimden bahsetmek istiyorum:
[Packages](packages.md)