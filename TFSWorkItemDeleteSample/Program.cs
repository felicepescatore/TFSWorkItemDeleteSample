using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TFSWorkItemsDeleteSample
{
    /// <summary>
    /// TFSWorkItemsDeleteSample
    /// ========================
    /// Attenzione: questo programma viene rilasciato senza alcuna garanzia e responsabilità da parte dell'autore.
    /// Ogni suo utilizzo implica la relativa assunzione di tutte le responsabilità.
    /// Per info: felice.pescatore@gmail.com
    /// </summary>
    class Program
    {
        //Proprio Account VSO
        private static string _vsoAddress = @"https://youraccount.visualstudio.com/DefaultCollection";
        //Progetto (Team Project) di riferimento
        private static string _teamProject = @"you project";
        //Area di riferimento contente i Work Items da canacellare;
        private static string _teamProjectArea = @"_ReadyToDelete";

        //Creadenziali di accesso all'account VSO
        private static string _userName = @"your username/account";
        private static string _password = @"your password";

        static void Main(string[] args)
        {
            //Segnalo la pericolosità dell'azione che si sta per eseguire e chiedo se si intende proseguire
            if (!AskForConfirm()) return;

            //Creo l'oggetto contenente le credenziali
            TfsClientCredentials cred = GetVSOCredentials(_userName, _password);

            //Apro la connessione verso VSO
            TfsTeamProjectCollection teamProjectCollection =
                new TfsTeamProjectCollection(new Uri(_vsoAddress), cred);

            //Effettuo l'autenticazione
            teamProjectCollection.Authenticate();
            WorkItemStore store = teamProjectCollection.GetService<WorkItemStore>();

            //Recupero l'ID dei Work Item dello specifico progetto e nella specifica area
            IEnumerable<int> workItemsId = GetWorkItemsIdByProjectAndArea(_teamProject, _teamProjectArea, store);

            //Procedo con la cancellazione
            DeleteWorkItems(store, workItemsId);
            Console.ReadKey();
        }


        /// <summary>
        /// Gets the vso credentials.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        private static TfsClientCredentials GetVSOCredentials(string userName, string password)
        {
            var basicCreds = new BasicAuthCredential(new NetworkCredential(userName, password));
            
            return new TfsClientCredentials(basicCreds) {AllowInteractive = false};
        }

        /// <summary>
        /// Gets the work items identifier by project and area.
        /// </summary>
        /// <param name="teamProject">The team project.</param>
        /// <param name="area">The area.</param>
        /// <param name="store">The store.</param>
        /// <returns></returns>
        private static IEnumerable<int> GetWorkItemsIdByProjectAndArea(String teamProject, String area, WorkItemStore store)
        {        
            String query = @"SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = '" + teamProject + "'  AND  [System.AreaPath] UNDER '" + teamProject + @"\" + area + "' ORDER BY [System.Id]";
            WorkItemCollection workItems = store.Query(query);
            return (from WorkItem wi in workItems
                    select wi.Id);
        }

        /// <summary>
        /// Deletes the work item.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <param name="workItemsId">The work items identifier.</param>
        private static void DeleteWorkItems(WorkItemStore store, IEnumerable<int> workItemsId) 
        {
            try
            {
                //Segnalo la cancellazione
                Console.Write("Procedo con la cancellazione dei seguenti WorkItem: ");
                foreach (int id in workItemsId)
                    Console.Write(id + ", ");
                Console.WriteLine();

                //Effettuo la cancellazione"
                IEnumerable deletedWorkItems = store.DestroyWorkItems(workItemsId);
                
                //Segnalo il completamente dell'opeazione
                Console.Write("WorkItem cancellati!");

                //Segnalo eventuali errori
                Console.Write("Eventali errori riscontrati:");
                foreach (WorkItemOperationError err in deletedWorkItems)
                     Console.WriteLine(err.Exception.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("La cancellazione non è andata a buon fine.");
                Console.WriteLine(ex.Message);
            }

        }

        /// <summary>
        /// Asks for confirm.
        /// </summary>
        /// <returns></returns>
        private static Boolean AskForConfirm()
        {
            Console.WriteLine("ATTENZIONE: l'operazione che si sta per eseguire è IRREVERSIBILE!!!");
            Console.WriteLine("Una volta cancellati, i work item non potranno essere recuperati!!!");
            Console.Write("Si desiera proseguire? [s/n] ");

            string response = Console.ReadLine();
            if (response.ToLower() == "s") return true;
            return false;
        }
    }
}
