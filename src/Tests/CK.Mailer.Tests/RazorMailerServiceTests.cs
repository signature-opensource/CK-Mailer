using CK.Core;
using CK.Mailer.Razor;
using FluentAssertions;
using MimeKit;
using MimeKit.Text;
using NUnit.Framework;
using RazorLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Mailer.Tests
{
    [TestFixture]
    public class RazorMailerServiceTests
    {
        [Test]
        public async Task send_email_from_razor_mail_sender_must_be_append_in_PickupDirectory()
        {
            RazorMailKitOptions options = DefaultMailKitOptions.DefaultRazor;

            IActivityMonitor m = FM.Get_ActivityMonitor();

            RazorMailModel<TemplateModel> mail = FM.Create_RazorMailModel_with_to_from_subject_body_generated_values();

            MailKitClientProvider clientProvider = new MailKitClientProvider( options );
            IRazorLightEngine razorEngine = EngineFactory.CreatePhysical( options.ViewsPhysicalPath );
            RazorMailerService service = new RazorMailerService( clientProvider, razorEngine );
            
            //send mail with BasicMailModel overload
            await service.SendInlineTemplateAsync( m, "Hello @Model.TheVariable", mail );

            var sentMail = PickupDirectory.GetTheLastSentEmail( options );

            sentMail.To.Cast<MailboxAddress>()
                .Select( x => x.Address )
                .ShouldBeEquivalentTo( 
                    mail.Recipients.To
                    .Cast<MailboxAddress>() 
                    .Select( x => x.Address )
                );
            sentMail.From.Cast<MailboxAddress>()
                .Select( x => x.Address )
                .ShouldBeEquivalentTo(
                    mail.Recipients.From
                    .Cast<MailboxAddress>()
                    .Select( x => x.Address )
                );
            sentMail.Subject.Should().Be( mail.Subject );
            sentMail.HtmlBody.Should().Be( $"Hello {mail.Model.TheVariable}" );
            sentMail.TextBody.Should().Be( $"Hello {mail.Model.TheVariable}" );
        }

        [Test]
        public async Task send_email_from_static_sender_with_MimeMessage_overload_must_be_append_in_PickupDirectory()
        {
            MailKitOptions options = DefaultMailKitOptions.Default;

            IActivityMonitor m = FM.Get_ActivityMonitor();

            BasicMailModel mail = FM.Create_BasicMailModel_with_to_from_subject_body_generated_values();

            //send mail with MimeMessage overload
            await StaticMailerService.SendMailAsync( m, mail.ToMimeMessage(), DefaultMailKitOptions.Default );

            var sentMail = PickupDirectory.GetTheLastSentEmail( options );

            sentMail.To.Cast<MailboxAddress>()
                .Select( x => x.Address )
                .ShouldBeEquivalentTo(
                    mail.Recipients.To
                    .Cast<MailboxAddress>()
                    .Select( x => x.Address )
                );
            sentMail.From.Cast<MailboxAddress>()
                .Select( x => x.Address )
                .ShouldBeEquivalentTo(
                    mail.Recipients.From
                    .Cast<MailboxAddress>()
                    .Select( x => x.Address )
                );
            sentMail.Subject.Should().Be( mail.Subject );
            sentMail.HtmlBody.Should().Be( mail.Body );
            sentMail.TextBody.Should().Be( mail.TextBody );
        }

        [Test]
        public async Task send_email_with_default_email_sender()
        {
            MailKitOptions options = DefaultMailKitOptions.Default;

            IActivityMonitor m = FM.Get_ActivityMonitor();

            BasicMailModel mail = FM.Create_BasicMailModel_without_from_mail_address();

            //send mail with MimeMessage overload
            await StaticMailerService.SendMailAsync( m, mail.ToMimeMessage(), DefaultMailKitOptions.Default );

            var sentMail = PickupDirectory.GetTheLastSentEmail( options );
            
            MailboxAddress senderAddress = sentMail.From.Cast<MailboxAddress>().Single();
            senderAddress.Address.Should().Be( options.DefaultSenderEmail );
            senderAddress.Name.Should().Be( options.DefaultSenderName );

            MailboxAddress receiverAddress = sentMail.To.Cast<MailboxAddress>().Single();
            receiverAddress.Address.Should().NotBe( options.DefaultSenderEmail );
            receiverAddress.Name.Should().NotBe( options.DefaultSenderName );

        }
        [Test]
        public async Task BasicMailModel_using_must_allow_the_automatic_html_escape_for_text_body()
        {
            MailKitOptions options = DefaultMailKitOptions.Default;
            IActivityMonitor m = FM.Get_ActivityMonitor();

            BasicMailModel mail = FM.Create_BasicMailModel_with_to_from_subject_body_generated_values();

            mail.Body = "&#8482;&#32;&#33; &#34; &#35; &#36; &#37; &#38; &#39; &#40; &#41; &#42; &#43; &#44; &#45; &#46; &#47; &#48; &#49; &#50; &#51; &#52; &#53; &#54; &#55; &#56; &#57; &#58; &#59; &#60; &#61; &#62; &#63; &#64; &#65; &#66; &#67; &#68; &#69; &#70; &#71; &#72; &#73; &#74; &#75; &#76; &#77; &#78; &#79; &#80; &#81; &#82; &#83; &#84; &#85; &#86; &#87; &#88; &#89; &#90; &#91; &#92; &#93; &#94; &#95; &#96; &#97; &#98; &#99; &#100; &#101; &#102; &#103; &#104; &#105; &#106; &#107; &#108; &#109; &#110; &#111; &#112; &#113; &#114; &#115; &#116; &#117; &#118; &#119; &#120; &#121; &#122; &#123; &#124; &#125; &#126; &#161; &#162; &#163; &#164; &#165; &#166; &#167; &#168; &#169; &#170; &#171; &#172; &#174; &#175; &#176; &#177; &#178; &#179; &#180; &#181; &#182; &#183; &#184; &#185; &#186; &#187; &#188; &#189; &#190; &#191; &#192; &#193; &#194; &#195; &#196; &#197; &#198; &#199; &#200; &#201; &#202; &#203; &#204; &#205; &#206; &#207; &#208; &#209; &#210; &#211; &#212; &#213; &#214; &#215; &#216; &#217; &#218; &#219; &#220; &#221; &#222; &#223; &#224; &#225; &#226; &#227; &#228; &#229; &#230; &#231; &#232; &#233; &#234; &#235; &#236; &#237; &#238; &#239; &#240; &#241; &#242; &#243; &#244; &#245; &#246; &#247; &#248; &#249; &#250; &#251; &#252; &#253; &#254; &#255; &#256; &#257; &#258; &#259; &#260; &#261; &#262; &#263; &#264; &#265; &#266; &#267; &#268; &#269; &#270; &#271; &#272; &#273; &#274; &#275; &#276; &#277; &#278; &#279; &#280; &#281; &#282; &#283; &#284; &#285; &#286; &#287; &#288; &#289; &#290; &#291; &#292; &#293; &#294; &#295; &#296; &#297; &#298; &#299; &#300; &#301; &#302; &#303; &#304; &#305; &#306; &#307; &#308; &#309; &#310; &#311; &#312; &#313; &#314; &#315; &#316; &#317; &#318; &#319; &#320; &#321; &#322; &#323; &#324; &#325; &#326; &#327; &#328; &#329; &#330; &#331; &#332; &#333; &#334; &#335; &#336; &#337; &#338; &#339; &#340; &#341; &#342; &#343; &#344; &#345; &#346; &#347; &#348; &#349; &#350; &#351; &#352; &#353; &#354; &#355; &#356; &#357; &#358; &#359; &#360; &#361; &#362; &#363; &#364; &#365; &#366; &#367; &#368; &#369; &#370; &#371; &#372; &#373; &#374; &#375; &#376; &#377; &#378; &#379; &#380; &#381; &#382; &#383;";
            
            string expected = "™ ! \" # $ % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? @ A B C D E F G H I J K L M N O P Q R S T U V W X Y Z [ \\ ] ^ _ ` a b c d e f g h i j k l m n o p q r s t u v w x y z { | } ~ ¡ ¢ £ ¤ ¥ ¦ § ¨ © ª « ¬ ® ¯ ° ± ² ³ ´ µ ¶ · ¸ ¹ º » ¼ ½ ¾ ¿ À Á Â Ã Ä Å Æ Ç È É Ê Ë Ì Í Î Ï Ð Ñ Ò Ó Ô Õ Ö × Ø Ù Ú Û Ü Ý Þ ß à á â ã ä å æ ç è é ê ë ì í î ï ð ñ ò ó ô õ ö ÷ ø ù ú û ü ý þ ÿ Ā ā Ă ă Ą ą Ć ć Ĉ ĉ Ċ ċ Č č Ď ď Đ đ Ē ē Ĕ ĕ Ė ė Ę ę Ě ě Ĝ ĝ Ğ ğ Ġ ġ Ģ ģ Ĥ ĥ Ħ ħ Ĩ ĩ Ī ī Ĭ ĭ Į į İ ı Ĳ ĳ Ĵ ĵ Ķ ķ ĸ Ĺ ĺ Ļ ļ Ľ ľ Ŀ ŀ Ł ł Ń ń Ņ ņ Ň ň ŉ Ŋ ŋ Ō ō Ŏ ŏ Ő ő Œ œ Ŕ ŕ Ŗ ŗ Ř ř Ś ś Ŝ ŝ Ş ş Š š Ţ ţ Ť ť Ŧ ŧ Ũ ũ Ū ū Ŭ ŭ Ů ů Ű ű Ų ų Ŵ ŵ Ŷ ŷ Ÿ Ź ź Ż ż Ž ž ſ";
            
            await StaticMailerService.SendMailAsync( m, mail, DefaultMailKitOptions.Default );

            var sentMail = PickupDirectory.GetTheLastSentEmail( options );

            sentMail.TextBody.Should().Be( expected );


            //TEST WITH THE OTHER SYNTAX
            mail.Body = "&euro;&nbsp;&quot; &amp; &lt; &gt; &iexcl; &cent; &pound; &curren; &yen; &brvbar; &sect; &uml; &copy; &ordf; &not; &reg; &macr; &deg; &plusmn; &sup2; &sup3; &acute; &micro; &para; &middot; &cedil; &sup1; &ordm; &raquo; &frac14; &frac12; &frac34; &iquest; &Agrave; &Aacute; &Acirc; &Atilde; &Auml; &Aring; &AElig; &Ccedil; &Egrave; &Eacute; &Ecirc; &Euml; &Igrave; &Iacute; &Icirc; &Iuml; &ETH; &Ntilde; &Ograve; &Oacute; &Ocirc; &Otilde; &Ouml; &times; &Oslash; &Ugrave; &Uacute; &Ucirc; &Uuml; &Yacute; &THORN; &szlig; &agrave; &aacute; &acirc; &atilde; &auml; &aring; &aelig; &ccedil; &egrave; &eacute; &ecirc; &euml; &igrave; &iacute; &icirc; &iuml; &eth; &ntilde; &ograve; &oacute; &ocirc; &otilde; &ouml; &divide; &oslash; &ugrave; &uacute; &ucirc; &uuml; &yacute; &thorn;";

            expected = "€ \" & < > ¡ ¢ £ ¤ ¥ ¦ § ¨ © ª ¬ ® ¯ ° ± ² ³ ´ µ ¶ · ¸ ¹ º » ¼ ½ ¾ ¿ À Á Â Ã Ä Å Æ Ç È É Ê Ë Ì Í Î Ï Ð Ñ Ò Ó Ô Õ Ö × Ø Ù Ú Û Ü Ý Þ ß à á â ã ä å æ ç è é ê ë ì í î ï ð ñ ò ó ô õ ö ÷ ø ù ú û ü ý þ";
                
            await StaticMailerService.SendMailAsync( m, mail, DefaultMailKitOptions.Default );

            sentMail = PickupDirectory.GetTheLastSentEmail( options );

            sentMail.TextBody.Should().Be( expected );
        }
    }
}
