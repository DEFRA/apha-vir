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
        private readonly IMapper _mapper;

        public SubmissionService(ISubmissionRepository submissionRepository,
            ISampleRepository sampleRepository,
            IIsolateRepository isolatesRepository,
            IMapper mapper)
        {
            _submissionRepository = submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));
            _sampleRepository = sampleRepository ?? throw new ArgumentNullException(nameof(sampleRepository));
            _isolatesRepository = isolatesRepository ?? throw new ArgumentNullException(nameof(isolatesRepository));
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

        public async Task<string> SubmissionLetter(string AVNumber, string user)
        {
            var submission = await _submissionRepository.GetSubmissionDetailsByAVNumberAsync(AVNumber);
            var samples = await _sampleRepository.GetSamplesBySubmissionIdAsync(submission.SubmissionId);
            var isolates = await _isolatesRepository.GetIsolateInfoByAVNumberAsync(AVNumber);

            return await GenerateSubmissionLetter(submission, samples, isolates, user);
        }

        private async Task<string> GenerateSubmissionLetter(
            Submission submission,
            IEnumerable<Sample> samples,
            IEnumerable<IsolateInfo> isolates,
            string user
            )
        {
            string MissingText(object value) => value == null || string.IsNullOrWhiteSpace(value.ToString()) ? "[Missing]" : value.ToString();

            var NL = Environment.NewLine;
            var str = new StringBuilder();

            str.Append("Animal Health and Veterinary Laboratories Agency - Weybridge,").Append(NL);
            str.Append("Avian Virology, New Haw, Surrey KT15 3NB United Kingdom").Append(NL);
            str.Append("Facsimile +44 (0)1932 357856").Append(NL);
            str.Append(NL).Append(NL);
            str.Append("Date: ").Append(DateTime.Now.ToLongDateString()).Append(NL);
            str.Append(NL).Append(NL);
            str.Append(submission.Sender).Append(NL);
            str.Append(submission.SenderOrganisation).Append(NL);
            str.Append(submission.SenderAddress).Append(NL);
            str.Append(submission.SubmittingCountryName).Append(NL);
            str.Append("Our Ref: ").Append(submission.Avnumber).Append(NL);
            str.Append("Your Submission Ref: ").Append(submission.SendersReferenceNumber).Append(NL);
            str.Append("Date of Receipt: ").Append(submission.DateSubmissionReceived?.ToString("dd MMMM yyyy") ?? "").Append(NL);
            str.Append(NL);
            str.Append("Dear ").Append(submission.Sender).Append(NL).Append(NL);
            str.Append("With regards to your recent submission of samples the following detail(s) were omitted. ").Append(NL).Append(NL);
            str.Append("Country of Virus Origin: ").Append(MissingText(submission.CountryOfOriginName)).Append(NL).Append(NL);

            if (samples == null || samples.Count() == 0)
            {
                str.Append("Your Sample Ref: ").Append('\t').Append("[Missing]").Append(NL);
                str.Append("Species/Group: ").Append('\t').Append(MissingText(null)).Append(NL);
                str.Append("Virus Year of Isolation: ").Append(MissingText(null)).Append(NL).Append(NL);
            }
            else
            {
                foreach (var samp in samples)
                {
                    str.Append("Your Sample Ref: ").Append('\t').Append(samp.SenderReferenceNumber ?? "[Missing]").Append(NL);
                    str.Append("Species/Group: ").Append('\t').Append(MissingText(samp.HostSpeciesName)).Append(NL);

                    bool found = false;
                    if (isolates == null || isolates.Count() == 0)
                    {
                        // Assume SampleTypeName is not available, so just use "Virus Year of Isolation"
                        str.Append("Virus Year of Isolation: ").Append(MissingText(null)).Append(NL);
                    }
                    else
                    {
                        foreach (var iso in isolates)
                        {
                            // SampleId/ID mapping: using SampleId from Sample and Isolate
                            if (!found && iso.IsolateSampleId == samp.SampleId)
                            {
                                // SampleTypeName is not available, so just use "Virus Year of Isolation"
                                str.Append("Virus Year of Isolation: ").Append(MissingText(iso.YearOfIsolation)).Append(NL);
                                found = true;
                            }
                        }
                        if (!found)
                        {
                            str.Append("Virus Year of Isolation: ").Append(MissingText(null)).Append(NL);
                        }
                    }
                    str.Append(NL);
                }
            }

            str.Append(NL);
            str.Append("It is essential that we have these details and I would appreciate it if you could send the information to me as soon as possible. ");
            str.Append("Full and final analysis of your samples may be delayed until this information is received.").Append(NL).Append(NL).Append(NL);
            str.Append("Best Regards, ").Append(NL).Append(NL);
            str.Append(user);

            return str.ToString();
        }

    }
}
