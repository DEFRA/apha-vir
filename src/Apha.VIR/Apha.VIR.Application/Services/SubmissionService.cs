using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using System.Text;

namespace Apha.VIR.Application.Services
{
    public class SubmissionService : ISubmissionService
    {
        private readonly ISubmissionRepository _submissionRepository;
        private readonly ISampleRepository _sampleRepository;
        private readonly IIsolateRepository _isolatesRepository;
        private readonly ILookupRepository _lookupRepository;
        private readonly IMapper _mapper;
        private const string VirusYearOfIsolationLabel = "Virus Year of Isolation: ";

        public SubmissionService(ISubmissionRepository submissionRepository,
            ISampleRepository sampleRepository,
            IIsolateRepository isolatesRepository,
            ILookupRepository lookupRepository,
            IMapper mapper)
        {
            _submissionRepository = submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));
            _sampleRepository = sampleRepository ?? throw new ArgumentNullException(nameof(sampleRepository));
            _isolatesRepository = isolatesRepository ?? throw new ArgumentNullException(nameof(isolatesRepository));
            _lookupRepository = lookupRepository ?? throw new ArgumentNullException(nameof(lookupRepository));
            _mapper = mapper;
        }

        public async Task<bool> AVNumberExistsInVirAsync(string avNumber)
        {
            return await _submissionRepository.AVNumberExistsInVirAsync(avNumber);
        }

        public async Task<SubmissionDTO> GetSubmissionDetailsByAVNumberAsync(string avNumber)
        {
            var submission = await _submissionRepository.GetSubmissionDetailsByAVNumberAsync(avNumber);
            return _mapper.Map<SubmissionDTO>(submission);
        }

        public async Task AddSubmissionAsync(SubmissionDTO submission, string user)
        {
            var submissionData = _mapper.Map<Submission>(submission);
            await _submissionRepository.AddSubmissionAsync(submissionData, user);
        }

        public async Task UpdateSubmissionAsync(SubmissionDTO submission, string user)
        {
            var submissionData = _mapper.Map<Submission>(submission);
            await _submissionRepository.UpdateSubmissionAsync(submissionData, user);
        }

        public async Task DeleteSubmissionAsync(Guid submissionId, string userId, byte[] lastModified)
        {
            await _submissionRepository.DeleteSubmissionAsync(submissionId, userId, lastModified);
        }

        public async Task<IEnumerable<string>> GetLatestSubmissionsAsync()
        {
            var avNumbers = await _submissionRepository.GetLatestSubmissionsAsync();
            return avNumbers;
        }

        public async Task<string> SubmissionLetter(string AVNumber, string user)
        {
            var submission = await _submissionRepository.GetSubmissionDetailsByAVNumberAsync(AVNumber);
            var samples = await _sampleRepository.GetSamplesBySubmissionIdAsync(submission.SubmissionId);
            var isolates = await _isolatesRepository.GetIsolateInfoByAVNumberAsync(AVNumber);
            var samplesDto = _mapper.Map<IEnumerable<SampleDTO>>(samples);
            var hostSpecies = await _lookupRepository.GetAllHostSpeciesAsync();
            foreach (var sample in samplesDto) 
            {
                sample.HostSpeciesName = hostSpecies?.FirstOrDefault(wg => wg.Id == sample?.HostSpecies.GetValueOrDefault())?.Name;
            }
            return GenerateSubmissionLetter(submission, samplesDto, isolates, user);
        }

        private static string GenerateSubmissionLetter(
            Submission submission,
            IEnumerable<SampleDTO> samples,
            IEnumerable<IsolateInfo> isolates,
            string user
        )
        {
            static string MissingText(object? value) =>
                string.IsNullOrWhiteSpace(value?.ToString()) ? "[Missing]" : value.ToString()!;

            var NL = Environment.NewLine;
            var str = new StringBuilder();

            AppendHeader(str, NL);
            AppendSubmissionDetails(str, submission, NL);
            AppendGreeting(str, submission, NL);
            AppendCountryOfOrigin(str, submission, MissingText, NL);
            AppendSampleDetails(str, samples, isolates, MissingText, NL);
            AppendFooter(str, user, NL);

            return str.ToString();
        }

        private static void AppendHeader(StringBuilder str, string NL)
        {
            str.Append("Animal Health and Veterinary Laboratories Agency - Weybridge,").Append(NL);
            str.Append("Avian Virology, New Haw, Surrey KT15 3NB United Kingdom").Append(NL);
            str.Append("Facsimile +44 (0)1932 357856").Append(NL).Append(NL).Append(NL);
            str.Append("Date: ").Append(DateTime.Now.ToLongDateString()).Append(NL).Append(NL).Append(NL);
        }

        private static void AppendSubmissionDetails(StringBuilder str, Submission submission, string NL)
        {
            str.Append(submission.Sender).Append(NL);
            str.Append(submission.SenderOrganisation).Append(NL);
            str.Append(submission.SenderAddress).Append(NL);
            str.Append(submission.SubmittingCountryName).Append(NL);
            str.Append("Our Ref: ").Append(submission.Avnumber).Append(NL);
            str.Append("Your Submission Ref: ").Append(submission.SendersReferenceNumber).Append(NL);
            str.Append("Date of Receipt: ").Append(submission.DateSubmissionReceived?.ToString("dd MMMM yyyy") ?? "").Append(NL).Append(NL);
        }

        private static void AppendGreeting(StringBuilder str, Submission submission, string NL)
        {
            str.Append("Dear ").Append(submission.Sender).Append(NL).Append(NL);
            str.Append("With regards to your recent submission of samples the following detail(s) were omitted. ").Append(NL).Append(NL);
        }

        private static void AppendCountryOfOrigin(StringBuilder str, Submission submission, Func<object?, string> MissingText, string NL)
        {
            str.Append("Country of Virus Origin: ").Append(MissingText(submission.CountryOfOriginName)).Append(NL).Append(NL);
        }

        private static void AppendSampleDetails(StringBuilder str, IEnumerable<SampleDTO> samples, IEnumerable<IsolateInfo> isolates, Func<object?, string> MissingText, string NL)
        {
            if (samples == null || !samples.Any())
            {
                str.Append("Your Sample Ref: ").Append('\t').Append("[Missing]").Append(NL);
                str.Append("Species/Group: ").Append('\t').Append(MissingText(null)).Append(NL);
                str.Append(VirusYearOfIsolationLabel).Append(MissingText(null)).Append(NL).Append(NL);
                return;
            }

            foreach (var samp in samples)
            {
                str.Append("Your Sample Ref: ").Append('\t').Append(samp.SenderReferenceNumber ?? "[Missing]").Append(NL);
                str.Append("Species/Group: ").Append('\t').Append(MissingText(samp.HostSpeciesName)).Append(NL);

                AppendIsolationYear(str, samp, isolates, MissingText, NL);
                str.Append(NL);
            }
        }

        private static void AppendIsolationYear(StringBuilder str, SampleDTO samp, IEnumerable<IsolateInfo> isolates, Func<object?, string> MissingText, string NL)
        {
            if (isolates == null || !isolates.Any())
            {
                str.Append(VirusYearOfIsolationLabel).Append(MissingText(null)).Append(NL);
                return;
            }

            bool found = false;
            foreach (var iso in isolates)
            {
                if (!found && iso.IsolateSampleId == samp.SampleId)
                {
                    str.Append(VirusYearOfIsolationLabel).Append(MissingText(iso.YearOfIsolation)).Append(NL);
                    found = true;
                }
            }

            if (!found)
            {
                str.Append(VirusYearOfIsolationLabel).Append(MissingText(null)).Append(NL);
            }
        }

        private static void AppendFooter(StringBuilder str, string user, string NL)
        {
            str.Append(NL);
            str.Append("It is essential that we have these details and I would appreciate it if you could send the information to me as soon as possible. ");
            str.Append("Full and final analysis of your samples may be delayed until this information is received.").Append(NL).Append(NL).Append(NL);
            str.Append("Best Regards, ").Append(NL).Append(NL);
            str.Append(user);
        }

    }
}
