using Netsmart.Bedrock.CareFabric.Cdm.Entities;
using Netsmart.Bedrock.Rigging;
using NodaTime;
using NodaTime.Extensions;
using RH.Apollo.Contracts.Models;
using RH.Apollo.Persistence.Search;
using ApolloDateRange = RH.Apollo.Contracts.Models.DateRange;
using DateRange = Netsmart.Bedrock.CareFabric.Cdm.Entities.DateRange;
using Diagnosis = Netsmart.Bedrock.CareFabric.Cdm.Entities.Diagnosis;

namespace TestConsole.Netsmart;

public class NetsmartTest
{
    private readonly ICareFabricClientFactory _clientFactory;
    private readonly FileValueExtractorFactory _extractorFactory;

    public NetsmartTest(ICareFabricClientFactory clientFactory, FileValueExtractorFactory extractorFactory)
    {
        _clientFactory = clientFactory;
        _extractorFactory = extractorFactory;
    }

    public async Task Execute(CancellationToken cancellationToken)
    {
        var client = _clientFactory.Create();

        var startDate = LocalDate.FromDateTime(new DateTime(1980, 1, 1));
        var endDate = LocalDate.FromDateTime(DateTime.Now.Add(TimeSpan.FromDays(1)));

        var activeRange = new ApolloDateRange(startDate, endDate);

        var patientsRequest = new ListClientInputPayload
        {

        };

        var patients = await client.GetFeed<ListClientOutputPayload>(patientsRequest.ToPayload())
            .ReadAllAsync(e => e.SelectedClients, cancellationToken);

        var allergiesRequest = new ListAllergyInputPayload
        {
            ClientID = new CareFabricID("7", "RHMOBDEV1:DEV"),
            //ActiveDateRange = activeRange.ToCareFabric(),
        };

        var allergies = await client.GetFeed<ListAllergyOutputPayload>(allergiesRequest.ToPayload())
            .ReadAllAsync(e => e.SelectedAllergies, cancellationToken);

        //const string rootPath = @"F:\Workarea\NetSmartExtraction";
        //await using var noteExtractor = _extractorFactory.Create<ProgressNote>(@$"{rootPath}\ProgressNote.tsv");
        //await using var programExtractor = _extractorFactory.Create<NetsmartProgram>(@$"{rootPath}\Program.tsv");
        //await using var appointmentSummaryExtractor = _extractorFactory.Create<AppointmentSummary>(@$"{rootPath}\AppointmentSummary.tsv");
        //await using var appointmentExtractor = _extractorFactory.Create<Appointment>(@$"{rootPath}\Appointment.tsv");
        //await using var clientExtractor = _extractorFactory.Create<Client>(@$"{rootPath}\Client.tsv");
        //await using var episodeExtractor = _extractorFactory.Create<BehavioralHealthEpisode>(@$"{rootPath}\Episode.tsv");
        //await using var admissionsExtractor = _extractorFactory.Create<ProgramAdmission>(@$"{rootPath}\ProgramAdmission.tsv");
        //await using var diagnosisExtractor = _extractorFactory.Create<Diagnosis>(@$"{rootPath}\Diagnosis.tsv");
        //await using var insurancesExtractor = _extractorFactory.Create<InsurancePolicy>(@$"{rootPath}\InsurancePolicy.tsv");
        //await using var problemExtractor = _extractorFactory.Create<Problem>(@$"{rootPath}\Problem.tsv");
        //await using var carePlanExtractor = _extractorFactory.Create<CarePlanSummary>(@$"{rootPath}\CarePlanSummary.tsv");
        //await using var serviceCodesExtractor = _extractorFactory.Create<ValueSet>(@$"{rootPath}\ServiceCodes.tsv");
        //await using var staffMemberExtractor = _extractorFactory.Create<StaffMember>(@$"{rootPath}\StaffMember.tsv");
        //await using var systemAccountExtractor = _extractorFactory.Create<SystemAccountSummary>(@"F:\SystemAccountSummary.tsv");
        //await using var locationsExtractor = _extractorFactory.Create<ServiceLocation>(@$"{rootPath}\ServiceLocations.tsv");
        //await using var customProgressNoteExtractor = _extractorFactory.Create<AssessmentDisplayPlan>(@$"{rootPath}\ProgressNoteCustomDisplayPlan.tsv");

        var admissionsRequest = new ListProgramAdmissionInputPayload
        {
            ActiveDateRange = activeRange.ToCareFabric(),
            AdmissionDateRange = new() { FromDate = new DateTime(1980, 1, 1).ToCareFabric() },
            ClientID = new CareFabricID("626"),
        };

        var admissions = await client.GetFeed<ListProgramAdmissionOutputPayload>(admissionsRequest.ToPayload())
            .ReadAllAsync(e => e.SelectedProgramAdmissions, cancellationToken);

        foreach (var admission in admissions)
        {
            var dxRequest = new ListDiagnosisInputPayload
            {
                ClientID = new CareFabricID("626"), // Baby Test
                PersonalInteractionID = new CareFabricID(admission.BehavioralHealthEpisodeID.ID.Split("|").Last())
            };

            var dxResponse = await client.GetFeed<ListDiagnosisOutputPayload>(dxRequest.ToPayload())
                .ReadAllAsync(d => d.SelectedDiagnoses ?? new List<Diagnosis>(), cancellationToken);

            //await diagnosisExtractor.ExtractListAsync(dxResponse);
        }

        var searchCarePlanRequest2 = new SearchCarePlanInputPayload
        {
            PageSize = 10,
            PageIndex = 0,
            ClientID = new CareFabricID("100000496"),
            //ActiveDateRange = activeRange.ToCareFabric(),
            // Required
            PlanDateRange = new DateRange { FromDate = new DateTime(1980, 1, 1).ToCareFabric(), ToDate = DateTime.Now.ToCareFabric() },
        };

        var carePlans2 = await client.GetFeed<SearchCarePlanOutputPayload>(searchCarePlanRequest2.ToPayload())
            .ReadAllAsync(e => e.SearchResults, cancellationToken);

        //await carePlanExtractor.ExtractListAsync(carePlans);

        Console.WriteLine($"CarePlans: {carePlans2.Count} (unexpected: {carePlans2.Count(x => !activeRange.Overlaps(x.PlanDate, null))})");


        var codesRequest = new GetValueSetInputPayload
        {
            RequestedValueSetCode = new CodedEntry
            {
                DisplayName = "ServiceCodes"
            },
        };

        var serviceCodes = await client.GetFeed<GetValueSetOutputPayload>(codesRequest.ToPayload())
            .ReadAllAsync(p => p.SelectedValueSet?.Values ?? Enumerable.Empty<CodedEntry>(), cancellationToken);

        var customNoteRequest = new GetAssessmentDisplayPlanInputPayload
        {
        };

        var customNote = await client.RequestAsync<GetAssessmentDisplayPlanOutputPayload>(customNoteRequest.ToPayload("GetProgressNoteCustomDisplayPlan"), cancellationToken);

        var programRequest = new ListProgramInputPayload();

        var programs = await client.GetFeed<ListProgramOutputPayload>(programRequest.ToPayload())
            .ReadAllAsync(p => p.SelectedPrograms, cancellationToken);

        //await programExtractor.ExtractListAsync(programs);

        //if (customNote.SelectedAssessmentDisplayPlan.AssessmentDisplayPlanID != null)
        //    await customProgressNoteExtractor.ExtractObjectAsync(customNote.SelectedAssessmentDisplayPlan);

        var apptStatusRequest = new ListValueSetInputPayload
        {
            ValueSetCodes = new List<CodedEntry>
            {
                new CodedEntry { DisplayName = "AppointmentCarePOVStatusCodes" }
            }
        };

        var apptStatusResponse = await client.RequestAsync<ListValueSetOutputPayload>(apptStatusRequest.ToPayload("ListValueSets"), cancellationToken);
        Console.WriteLine($"Appt Statuses: {apptStatusResponse.ValueSets.Count}, Values: {apptStatusResponse.ValueSets.First().Values.Count}");

        var statuses = apptStatusResponse.ValueSets[0].Values.Select(v => v.DisplayName).ToList();

        var guarantorsRequest = new ListValueSetInputPayload
        {
            ValueSetCodes = new List<CodedEntry>
            {
                new CodedEntry { DisplayName = "Guarantors" }
            }
        };

        var guarantorsResponse = await client.RequestAsync<ListValueSetOutputPayload>(guarantorsRequest.ToPayload("ListValueSets"), cancellationToken);
        Console.WriteLine($"Guarantors: {guarantorsResponse.ValueSets?.Count}, Values: {guarantorsResponse.ValueSets?.FirstOrDefault()?.Values.Count}");

        //await locationsExtractor.ExtractListAsync(locationResponse.SelectedServiceLocations);

        var searchCaseClient = new SearchCaseClientInputPayload
        {
            PageSize = 10,
            PageIndex = 0,
            ProviderID = new("PMSYSADM")
        };

        var casePage = await client.RequestAsync<SearchCaseClientOutputPayload>(searchCaseClient.ToPayload(), cancellationToken);

        for (var letter = 'A'; letter <= 'Z'; letter++)
        {
            Console.WriteLine($"*************** Searching Clients by letter {letter} ***************");
            Console.WriteLine();

            var searchClient = new SearchClientInputPayload
            {
                PageSize = 10,
                PageIndex = 0,
                ClientName = new PersonName
                {
                    Last = letter.ToString()
                }
            };

            // Request just the first page of results
            var firstPage = await client.RequestAsync<SearchClientOutputPayload>(searchClient.ToPayload(), cancellationToken);

            var clientIds = firstPage.SearchResults.Select(p => (p.ClientID, p.ClientName));

            foreach (var (clientId, name) in clientIds)
            {
                try
                {
                    Console.WriteLine($"Patient: '{name.FirstMiddleLast}' (id: {clientId.ID})");

                    var patientRequest = new GetClientInputPayload
                    {
                        RequestedClientID = clientId,
                    };

                    var patientResponse = await client.RequestAsync<GetClientOutputPayload>(patientRequest.ToPayload(), cancellationToken);

                    //await clientExtractor.ExtractObjectAsync(patientResponse.SelectedClient);
                    /*
                    var assocPersonRequest = new ListAssociatedPersonInputPayload
                    {
                        ClientID = clientId,
                    };

                    var associatedPeople = await client.GetFeed<ListAssociatedPersonOutputPayload>(assocPersonRequest.ToPayload())
                        .ReadAllAsync(e => e.SelectedAssociatedPeople, cancellationToken);

                    if (associatedPeople.Count > 0)
                        Console.WriteLine($"Associated People: {associatedPeople.Count}");
                    */
                    /*
                    var insuranceRequest = new ListInsurancePolicyInputPayload
                    {
                        ClientID = clientId,
                        ActiveDateRange = activeRange.ToCareFabric()
                    };

                    var insurances = await client.GetFeed<ListInsurancePolicyOutputPayload>(insuranceRequest.ToPayload())
                        .ReadAllAsync(e => e.SelectedInsurancePolicies, cancellationToken);

                    //await insurancesExtractor.ExtractListAsync(insurances);

                    Console.WriteLine($"Insurances: {insurances.Count} (unexpected: {insurances.Count(x => !activeRange.Overlaps(x.EffectiveDate, x.ExpirationDate))})");
                    */
                    /*
                    var admissionsRequest = new ListProgramAdmissionInputPayload
                    {
                        ActiveDateRange = activeRange.ToCareFabric(),
                        AdmissionDateRange = new() { FromDate = new DateTime(1980, 1, 1).ToCareFabric() },
                        ClientID = clientId,
                    };

                    var admissions = await client.GetFeed<ListProgramAdmissionOutputPayload>(admissionsRequest.ToPayload())
                        .ReadAllAsync(e => e.SelectedProgramAdmissions, cancellationToken);
                    
                    //await admissionsExtractor.ExtractListAsync(admissions);

                    Console.WriteLine($"Admissions: {admissions.Count} (unexpected: {admissions.Count(x => !activeRange.Overlaps(x.AdmissionDate, x.DischargeDate))})");
                    */
                    /*
                    var problemRequest = new ListProblemInputPayload
                    {
                        PageSize = 10,
                        PageIndex = 0,
                        ClientID = clientId,
                        ActiveDateRange = activeRange.ToCareFabric()
                    };

                    var problems = await client.GetFeed<ListProblemOutputPayload>(problemRequest.ToPayload())
                        .ReadAllAsync(e => e.SelectedProblems, cancellationToken);
                    
                    //if (problems != null)
                    //    await problemExtractor.ExtractListAsync(problems);

                    Console.WriteLine($"Problems: {problems.Count} (unexpected: {problems.Count(x => !activeRange.Overlaps(x.StartDate, x.EndDate))})");
                    */

                    /*
                    var notesRequest = new ListProgressNoteInputPayload
                    {
                        PageSize = 10,
                        PageIndex = 0,
                        ClientID = clientId,
                    };

                    var notes = await client.GetFeed<ListProgressNoteOutputPayload>(notesRequest.ToPayload())
                        .ReadAllAsync(e => e.SelectedProgressNotes, cancellationToken);

                    //if (notes != null)
                    //    await noteExtractor.ExtractListAsync(notes);

                    Console.WriteLine($"Notes: {notes.Count}");
                    */
                    var searchCarePlanRequest = new SearchCarePlanInputPayload
                    {
                        PageSize = 10,
                        PageIndex = 0,
                        ClientID = clientId,
                        IsActive = true,
                        ActiveDateRange = activeRange.ToCareFabric(),
                        // Required
                        PlanDateRange = new DateRange { FromDate = new DateTime(1980, 1, 1).ToCareFabric(), ToDate = DateTime.Now.ToCareFabric() },
                    };

                    var carePlans = await client.GetFeed<SearchCarePlanOutputPayload>(searchCarePlanRequest.ToPayload())
                        .ReadAllAsync(e => e.SearchResults, cancellationToken);

                    //await carePlanExtractor.ExtractListAsync(carePlans);

                    Console.WriteLine($"CarePlans: {carePlans.Count} (unexpected: {carePlans.Count(x => !activeRange.Overlaps(x.PlanDate, null))})");

                    /*
                    Disabled because it returns "Invalid site" for avatar
                    var apptRequest = new ListAppointmentInputPayload
                    {
                        PageSize = 10,
                        PageIndex = 0,
                        ClientID = clientId,
                    };

                    var apptResponse = await client.RequestAsync<ListAppointmentOutputPayload>(apptRequest.ToPayload(), cancellationToken);
                    Console.WriteLine($"Appt: {apptResponse.SelectedAppointments?.Count}");
                    */

                    /*
                    foreach (var episode in episodes)
                    {
                        var dxRequest = new ListDiagnosisInputPayload
                        {
                            ClientID = clientId,
                            PersonalInteractionID = new CareFabricID(episode.BehavioralHealthEpisodeID.ID.Split("|").Last())
                        };

                        var dxResponse = await client.GetFeed<ListDiagnosisOutputPayload>(dxRequest.ToPayload())
                            .ReadAllAsync(d => d.SelectedDiagnoses ?? new List<Diagnosis>(), cancellationToken);

                        Console.WriteLine($"Diagnoses from episode {episode.BehavioralHealthEpisodeID.ID}: {dxResponse.Count}");
                    }
                    */
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }

                Console.WriteLine();
            }
        }

        for (var letter = 'A'; letter <= 'Z'; letter++)
        {
            Console.WriteLine($"*************** Searching Staff by letter {letter} ***************");
            Console.WriteLine();

            try
            {
                var searchStaff = new SearchStaffMemberInputPayload
                {
                    PageSize = 50,
                    PageIndex = 0,
                    Name = new PersonName
                    {
                        First = letter.ToString(),
                    }
                };

                // Request just the first page of results
                var staffSummary = await client.RequestAsync<SearchStaffMemberOutputPayload>(searchStaff.ToPayload(), cancellationToken);

                foreach (var entry in staffSummary.SearchResults)
                {
                    Console.WriteLine($"Staff: '{entry.StaffMemberName.FirstMiddleLast}' (id: {entry.StaffMemberID.ID})");

                    var staffRequest = new GetStaffMemberInputPayload
                    {
                        RequestedStaffMemberID = entry.StaffMemberID,
                    };

                    var staff = await client.GetFeed<GetStaffMemberOutputPayload>(staffRequest.ToPayload())
                        .ReadNextAsync(p => p.SelectedStaffMember, cancellationToken);

                    //await staffMemberExtractor.ExtractObjectAsync(staff);

                    /*
                    var systemAccountRequest = new SearchSystemAccountInputPayload
                    {
                        StaffMemberID = staff.StaffMemberID
                    };

                    var systemAccounts = await client.GetFeed<SearchSystemAccountOutputPayload>(systemAccountRequest.ToPayload())
                        .ReadNextAsync(p => p.SearchResults, cancellationToken);
                    */
                    //await systemAccountExtractor.ExtractListAsync(systemAccounts);

                    Console.WriteLine(string.IsNullOrWhiteSpace(entry.WorkEmailAccount?.Address) ? "no email" : entry.WorkEmailAccount?.Address);

                    foreach (var systemAccount in entry.SystemAccounts)
                    {
                        Console.WriteLine($"{systemAccount.Type} {systemAccount.Value}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            Console.WriteLine();
        }

        var vRequest = new ListValueSetInputPayload
        {
            ValueSetCodes = new List<CodedEntry>
            {
                new CodedEntry { DisplayName = "ServiceCodes" }
            }
        };

        var vResponse = await client.RequestAsync<ListValueSetOutputPayload>(vRequest.ToPayload("ListValueSets"), cancellationToken);
        Console.WriteLine($"HealthCareService: {vResponse.ValueSets?.Count}, Values: {vResponse.ValueSets?.FirstOrDefault()?.Values.Count}");

        //if (vResponse.ValueSets != null)
        //    await serviceCodesExtractor.ExtractListAsync(vResponse.ValueSets);
    }
}
