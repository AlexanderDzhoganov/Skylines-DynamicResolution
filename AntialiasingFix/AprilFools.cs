using System;
using System.Globalization;
using System.Reflection;
using ColossalFramework;
using ColossalFramework.Steamworks;
using ColossalFramework.UI;
using UnityEngine;

namespace DynamicResolution
{

    internal class CustomMessage : MessageBase
    {
        private readonly string _sender, _message;

        public CustomMessage(string sender, string message)
        {
            _sender = sender;
            _message = message;
        }

        public override string GetSenderName()
        {
            return _sender;
        }

        public override string GetText()
        {
            return _message;
        }

        public void Show()
        {
            Singleton<ChirpPanel>.instance.AddMessage(this, true);
        }
    }
   
    public class AprilFools : MonoBehaviour
    {

        public static bool IsAprilFools()
        {
            return (DateTime.Today.Day == 1 && DateTime.Today.Month == 4);
        }

        private static bool bootstrapped = false;

        public static void Bootstrap()
        {
            if (!IsAprilFools())
            {
                return;
            }

            if (bootstrapped)
            {
                return;
            }

            ReplaceLogo();

            RedirectionHelper.RedirectCalls
            (
                typeof(WorkshopAdPanel).GetMethod("OnQueryCompleted",
                    BindingFlags.Instance | BindingFlags.NonPublic),
                typeof(AprilFools).GetMethod("WorkshopAdPanelOnQueryCompleted",
                    BindingFlags.Instance | BindingFlags.NonPublic)
            );

            bootstrapped = true;
        }

        private static string[] chirps = new string[]
        {
            "Maxis was founded in 1987 by Will Wright and Jeff Braun to help publish SimCity on home computers. Before then, the game was only available on a limited basis on the Commodore 64 due to few publishers showing any interest in porting it.",
            "After the immense success of SimCity, Maxis experimented with different genres. However, their new games, including The Crystal Skull and SimCopter, were commercial failures.",
            "In March 2015, it was revealed by Guillaume Pierre (Lead Gameplay Scripter - SimCity) that the Maxis Emeryville studio was being closed.",
            "Maxis is widely regarded for its innovative simulation games, in which there is no specific goal to the player, making its games almost endless.",
            "Maxis' most successful series to date and the best-selling PC game of all time is The Sims (2000).[13] Maxis has developed seven expansion packs for the game as well as an online version (The Sims Online).",
            "EA completed its acquisition of Maxis on July 28, 1997. Compared to other companies acquired by EA, such as Origin Systems and Westwood Studios, the absorption of Maxis took a slower pace, and the company staff was lost only gradually.",
            "Over 1998 Maxis was allowed to finish SimCity 3000 on its own time; following this, Wright's efforts were thrown into The Sims, at the time seen as a major gamble for the company, as the dollhouse game was not seen as a match for the video game market's demographics.",
            "The Sims was released in February 2000; its massive success buoyed Wright's reputation and saved Maxis as a separate working unit.",
            "After building his reputation as one of the most important game designers in the world, Will Wright left Maxis in 2009. ",
            "When Will Wright took his idea for The Sims to the Maxis board of directors, Jeff Braun said, 'The board looked at it and said, ‘What is this? He wants to do an interactive doll house? The guy is out of his mind. Doll houses were for girls, and girls didn’t play video games.'",
            "SimCity was originally developed by game designer Will Wright. The inspiration for SimCity came from a feature of the game Raid on Bungeling Bay that allowed Wright to create his own maps during development. Wright soon found he enjoyed creating maps more than playing the actual game, and SimCity was born",
            "As The Sims became a steady success, Will Wright began to focus on Spore. With the spin-off of The Sims Studio, it was Maxis' only project (In 2006, Maxis.com simply redirected to The Sims). The three years between its public announcement and its release were protracted enough to attract use of the term \"vaporware\" by some, and upon its 2008 release, found itself subject to harsh criticism and the target of a consumer protest against Electronic Arts. Despite the poor launch publicity, Spore still sold 1 million units in its first month.",
            "Will Wright left Maxis in 2009. Maxis' only new standalone title until 2013 was Darkspore. During this time, Maxis.com redirected to the Spore website, then later once more to the website for The Sims. Throughout this period, the studio continued to operate in Emeryville.",
            "At the 2012 Game Developers Conference, EA announced a new SimCity along with a new logo for the Maxis brand. Maxis became one of four primary labels at EA, replacing the \"EA Play\" brand. Development of The Sims continued with The Sims Studio, which was reintegrated with Maxis in 2012, and Maxis branding returned in 2013 with the launch of The Sims 3: University Life, and SimCity (2013).",
            "The initial release of SimCity on March 5, 2013 in North America suffered multiple severe issues, particularly regarding the game's requirement for a persistent Internet connection. After the game was made available for purchase through EA's Origin delivery service, the high volume of users attempting to download and connect to EA's game servers caused network outages. Players reported experiencing frequent problems during gameplay due to the outages such as long loading times, disconnections, crashing, and loss of saved game data.",
            "SimCity 2013's launch failures also led to fans of the series filing a petition with We the People (petitioning system) on the official White House website calling for \"an industry-wide return policy for video games that rely on remote servers and DRM to function properly\" which was later covered by mainstream news organizations such as NBC News."
        };

        private float timeToNextChirp = 600.0f;

        void Update()
        {
            timeToNextChirp -= Time.deltaTime;
            if (timeToNextChirp <= 0.0f)
            {
                var chirpIndex = UnityEngine.Random.Range(0, chirps.Length);
                try
                {
                    new CustomMessage("MaxisFacts", chirps[chirpIndex]).Show();
                }
                catch (Exception)
                {
                }

                timeToNextChirp = UnityEngine.Random.Range(600.0f, 3600.0f);
            }
        }

        public static void Revert()
        {
            if (!bootstrapped)
            {
                return;
            }


            bootstrapped = false;
        }

        public static byte[] GetResource(string name)
        {
            var asm = Assembly.GetExecutingAssembly();
            var stream = asm.GetManifestResourceStream(name);
            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, (int)stream.Length);
            return data;
        }

        private static void AddChirps()
        {
            
        }

        private static void ReplaceLogo()
        {
            var menuContainer = GameObject.Find("MenuContainer").GetComponent<UIPanel>();
            var logo = menuContainer.Find<UITextureSprite>("Logo");
            var newLogo = new Texture2D(858, 343);

            newLogo.LoadImage(GetResource("DynamicResolution.Resources.cimcity.png"));

            logo.texture = newLogo;

            logo.size = new Vector2(800, 320);
            logo.relativePosition = new Vector3(logo.relativePosition.x, -240.0f, logo.relativePosition.z);
        }

        private void WorkshopAdPanelOnQueryCompleted(UGCDetails result, bool ioError)
        {
            try
            {
                var workshopAdPanel = GameObject.Find("WorkshopAdPanel").GetComponent<WorkshopAdPanel>();

                if (result.result == Result.OK)
                {
                    UIComponent uIComponent = Util.GetPrivate<UIScrollablePanel>(workshopAdPanel, "m_ScrollContainer").AttachUIComponent
                        (UITemplateManager.GetAsGameObject("WorkshopAdTemplate"));

                    string price = String.Format(CultureInfo.CurrentCulture, "{0:C}", 0.99f);
                    if (UnityEngine.Random.Range(0, 2) == 0)
                    {
                        price = String.Format(CultureInfo.CurrentCulture, "{0:C}", 2.49f);
                    }
                    else if (UnityEngine.Random.Range(0, 7) == 0)
                    {
                        price = String.Format(CultureInfo.CurrentCulture, "{0:C}", 4.99f);
                    }
                    else if (UnityEngine.Random.Range(0, 25) == 0)
                    {
                        price = String.Format(CultureInfo.CurrentCulture, "{0:C}", 8.49f);
                    }

                    uIComponent.Find<UILabel>("Title").text = String.Format("{0} - {1}\n{2}", result.title, price, result.tags);

                    if (result.image != null)
                    {
                        result.image.wrapMode = TextureWrapMode.Clamp;
                    }
                    uIComponent.Find<UITextureSprite>("Image").texture = result.image;
                    UIProgressBar uIProgressBar = uIComponent.Find<UIProgressBar>("Rating");
                    uIProgressBar.isVisible = (result.score >= 0f);
                    uIProgressBar.value = result.score;
                    uIComponent.Find<UIButton>("ClickableArea").eventClick += delegate(UIComponent c, UIMouseEventParameter p)
                    {
                        if (Steam.IsOverlayEnabled())
                        {
                            Steam.ActivateGameOverlayToWorkshopItem(result.publishedFileId);
                        }
                    };
                }
                else
                {
                    CODebugBase<LogChannel>.Warn(LogChannel.Core, string.Concat(new object[]
				{
					"Workshop item: ",
					result.publishedFileId,
					" error: ",
					result.result
				}));
                }
            }
            catch (Exception ex)
            {
                CODebugBase<LogChannel>.Error(LogChannel.Core, ex.ToString());
            }
        }

    }

}
