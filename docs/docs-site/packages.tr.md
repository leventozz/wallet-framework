Hangi paketi neden kullandım?

Bu bölümde hangi paketi neden tercih ettiğimi açıklamak istiyorum. Aynı zamanda bu kısım framework’ün en tartışmaya açık bölümü olduğu için görüş belirtmekten çekinmeyin.

**MassTransit:**
Yakın zamanda ücretli lisansa geçileceği duyuruldu. Bu nedenle kullanmamak için bir süre direndim. Ancak sektörde uzun süredir kendini kanıtlamış bir ürünü yalnızca bu sebeple göz ardı etmek mantıklı değildi. En büyük rakibi olan NServiceBus’ın kullanıldığı bir finans projesinde görev almıştım. NServiceBus’tan mı kaynaklıydı bilmiyorum ama yapı bana oldukça karmaşık gelmişti; neyin ne yaptığını anlamak için uzun süre uğraşmıştım.
MassTransit projede karşılaştığım bir sorunu çözmekte de doğrudan yardımcı oldu. Audit logları için işlemi yapan kullanıcının kimliğini alt katmanlara taşımak ciddi bir zorluk haline gelmişti. MassTransit middleware’i bu problemi çözmek için zaten hazır bekliyordu.

**YARP:**
Mikroservis mimarisinde gateway projesi için YARP’ı tercih ettim. Kullanımı son derece pratikti. YARP’ın tüm yeteneklerinin farkında olmadığımı ve tamamını kullanmadığımı biliyorum; ihtiyacım olan kadarıyla ilgilendim. Daha önce bir projede gateway olarak Ocelot kullanmıştım, ancak oldukça karmaşık gelmişti. Bu nedenle YARP’a şans verdim ve sonuçtan memnun kaldım.

**Keycloak:**
Lisans ücreti olmaması önemli bir etken oldu. Aynı zamanda dağıtık mimarilerde performansı oldukça iyi. Ancak projeye yeniden başlasam büyük ihtimalle tercih etmezdim; çünkü API kullanımı için güvenilir bir NuGet paketi bulamadım ve tüm işlemleri kendim yazmak zorunda kaldım. Bu hem zaman kaybı yarattı hem de gereksiz karmaşıklık oluşturdu.

Bunlar dışında kullandığım diğer ürünler çoğunuzun en az bir kere karşılaştığı yapılar olduğundan daha fazla detaya girmiyorum.

Şimdi isterseniz hangi problemlerle karşılaştığımdan bahsedeyim:
[Challenges](problems.md)
