with
/****************************************************************************************************************************
	History:
	Created: 2025-05-16:	YM001:	Ticket - 515337:	Masterscript - MasterScript - Master script to be split into Large producer master script and small producer master script
	Created: 2025-05-21:	YM002:	Ticket - 515336:	Masterscript - Addition of Transitional packaging Data in Large producer master script for 2024
	Created: 2025-05-28:	YM003:	Ticket - 549638:	Masterscript - Logic change for First and Latest Registration File Submissions for status queried
	Updated: 2025-05-30:	PM004:  Ticket - 515339:	Masterscript - Fix for flags Organisation visible in PowerBI Packaging reports, Organisation exists in most recent organisation data submission
	Updated: 2025-06-04:	YM005:  Ticket - 562694:	Masterscript - Removing Queried record if there are more than one Queried next to each other
	Updated: 2025-06-11:	YM006:  Ticket - 561770:	Masterscript - Check and update the Logic for First and Latest Registration File Submissions in master script - Registration resubmission
	Updated: 2025-06-11:	YM007:  Ticket - 548936:    Master script not to show resubmitted POM submission with "Uploaded" status
	Updated: 2025-07-08:	YM008:  Ticket - 569433:    Master script - Org size to show only Parent Organisation
	Updated: 2025-08-27:	PM009:  Ticket - 605220:    Master script - New column with RAM and RAM-M as new set of columns
	Updated: 2025-08-28:	AA010:  Ticket - 605105:    Master script - New 4 additional columns will be introduced to split the plastics to “Rigid & Flexible” on Large producer
	Updated: 2025-08-28:	PM011:  Ticket - 605220:    Master script - New columns with RAM and RAM-M For rigid, flexible columns added as part 605105
	Updated: 2025-09-01:	PM012:  Ticket - 607670:    Master script - Split file as small or Large for the year 2025
	Updated: 2025-10-22:	PM013:  Ticket - 624165:    Master script - Masterscript Bug -  Self-Managed Consumer Waste
	Updated: 2025-11-06:	PM014:  Ticket - RAM M:     Master script - Adding R G A split for each RAM and RAM-M column
	Updated: 2025-11-18:	PM015:  Ticket - 640727:    Master script - To handle BLANK value along with NULL records
	Updated: 2026-06-24:	CF016:  Paycal compatibility - "Latest" Registration/POM submission now means latest ACCEPTED submission
							(narrowed to GRANTED/ACCEPTED for Registration, ACCEPTED for POM), not latest regardless of status -
							matching sp_GetPaycalPomData/sp_GetPaycalOrgData. "First" submission selection semantics are unchanged.
							Also: the Registration (ORG) join to v_submitted_pom_org_file_status now matches on cfm_FileId+FileType
							(as Paycal does) instead of FileName, since that view dedupes by cfm_FileId - this affects the
							Regulator_Status/Actual_Regulator_Status used by both "first" and "latest" registration. (POM's
							equivalent join was replaced entirely by CF020 below.)
	Updated: 2026-06-24:	CF017:  Paycal compatibility - POM tonnage pivots now exclude subsidiaries that are not Obligated
							('O') per dbo.t_producer_obligation_determination, matching the subsidiary/submitter check Paycal applies.
	Updated: 2026-06-24:	CF021:  Bugfix - the CF017 obligation-status join was changed from INNER to a conditional LEFT join,
							scoped to organisation_size='L' only. v_producer_obligation_determination.sql (which feeds
							t_producer_obligation_determination) only ever computes obligation status for Large producers - the
							original INNER JOIN was wrongly treating every Small/Medium producer (and any historical year not yet
							covered by the table) as "not obligated", which a CSV diff against the original view showed was
							collapsing total POM tonnage by ~65%. Known residual risk: Large producers in historical years the
							table doesn't yet cover will still be wrongly excluded (t_producer_obligation_determination may be a
							partial/stale snapshot rather than the comprehensive live v_producer_obligation_determination view -
							querying the table instead of the view was a deliberate performance trade-off).
	Updated: 2026-06-24:	CF018:  Performance - POM data is only needed from 2024 onwards; pre-2024 SubmissionPeriods are now
							excluded as early as possible, reducing row counts through the rest of the POM pipeline.
							Registration (ORG) is unaffected - still full history. (Superseded/narrowed by CF022 below.)
	Updated: 2026-06-24:	CF022:  Performance - the consuming system only ever uses Large producer ('L') data for 2024/2025
							reporting years; everything else is discarded downstream regardless. Both ORG (registration) and
							POM now filter to organisation_size='L' and the relevant 2024/2025 SubmissionPeriods as early as
							possible (narrowing CF018's broader "2024+" prune), instead of computing and discarding Small/Medium
							producers and 2026+ periods. NOTE: this is a deliberate, scope-narrowing departure from "keep all
							organisation sizes/years, let the system filter downstream" - revisit if that consumption pattern
							changes.
	Updated: 2026-06-24:	CF020:  Performance - POM's Regulator_Status/IsResubmission_identifier are now computed inline
							(POM_submitted_file_list..POM_decision CTEs) instead of joining v_submitted_pom_org_file_status.
							That view's null-FileId backfill (sets A/B/C) is entirely registration-only but was being paid for
							on every POM query too, since it isn't filterable by FileType. Registration (ORG) still joins the
							real view, unchanged. This duplicates that view's Pom-branch logic, so the two must be kept in sync
							if v_submitted_pom_org_file_status's Pom handling changes in future.
	Updated: 2026-06-24:	CF019:  Performance - the tonnage pivots' new obligation-status join (CF017) now only considers the
							small set of FileNames f_pom_sql/l_pom_sql actually select (the only FileNames the final SELECT can
							ever reference), instead of scanning/joining every POM file ever submitted.
******************************************************************************************************************************/
TargetObligationYear as --CF023: single configurable constant - client currently only needs reporting/obligation year 2026 (POM submission year 2025). Change ObligationYear here to retarget a different cycle; every join below that scopes by year reads from this one place instead of a separately-hardcoded literal. TargetPomYear (ObligationYear - 1) is the POM/registration submission year that feeds that obligation year
(
	select 2026 as ObligationYear, 2026 - 1 as TargetPomYear
),
ValidSubmissionPeriodsForTarget as
(
	select SP from
	(
		select 'Jan to Jun 2023' as SP, 2023 as Year
		union all select 'January to June 2023', 2023
		union all select 'July to December 2023', 2023
		union all select 'Jan to Jun 2024', 2024
		union all select 'January to June 2024', 2024
		union all select 'July to December 2024', 2024
		union all select 'January to December 2025', 2024
		union all select 'Jan to Jun 2025', 2025
		union all select 'January to June 2025', 2025
		union all select 'July to December 2025', 2025
		union all select 'January to December 2026', 2025
		union all select 'Jan to Jun 2026', 2026
		union all select 'January to June 2026', 2026
		union all select 'July to December 2026', 2026
		union all select 'Jan to Jun 2027', 2027
		union all select 'January to June 2027', 2027
		union all select 'July to December 2027', 2027
		union all select 'Jan to Jun 2028', 2028
		union all select 'January to June 2028', 2028
		union all select 'July to December 2028', 2028
	) AllPeriods
	cross join TargetObligationYear toy
	where AllPeriods.Year = toy.TargetPomYear
),
TwoRow as --CF023: scoped to TargetPomYear - this drives base_sql's Organisation x Period fan-out below, so narrowing it here directly limits the view's final output to just the relevant period row(s) instead of all twelve
(
	select RankId, SP, Reporting_Year
	from
	(
		select 1 as RankId , 'Jan to June 2023 - H1' as SP, 2023 as Reporting_Year
		union
		select 2 as RankId , 'July to Dec 2023 - H2' as SP, 2023 as Reporting_Year
		union
		select 3 as RankId , 'Jan to June 2024 - H1' as SP, 2024 as Reporting_Year
		union
		select 4 as RankId , 'July to Dec 2024 - H2' as SP, 2024 as Reporting_Year
		union
		select 5 as RankId , 'Jan to June 2025 - H1' as SP, 2025 as Reporting_Year
		union
		select 6 as RankId , 'July to Dec 2025 - H2' as SP, 2025 as Reporting_Year
		union
		select 7 as RankId , 'Jan to June 2026 - H1' as SP, 2026 as Reporting_Year
		union
		select 8 as RankId , 'July to Dec 2026 - H2' as SP, 2026 as Reporting_Year
		union
		select 9 as RankId , 'Jan to June 2027 - H1' as SP, 2027 as Reporting_Year
		union
		select 10 as RankId , 'July to Dec 2027 - H2' as SP, 2027 as Reporting_Year
		union
		select 11 as RankId , 'Jan to June 2028 - H1' as SP, 2028 as Reporting_Year
		union
		select 12 as RankId , 'July to Dec 2028 - H2' as SP, 2028 as Reporting_Year
	) AllTwoRow
	cross join TargetObligationYear toy
	where Reporting_Year = toy.TargetPomYear
),

ORG as
(
		select *
			, row_number() over(partition by OrganisationId, ReferenceNumber, SubmissionPeriod order by Submission_time asc, Source asc,cd_organisation_size desc) as First_submission
			, row_number() over(partition by OrganisationId, ReferenceNumber, SubmissionPeriod order by Submission_time desc, Source asc,cd_organisation_size desc) as Last_submission
		from
		(
			select distinct o.id as OrganisationId, cd.organisation_id as ReferenceNumber
					, case when cfm.SubmissionPeriod in ('Jan to Jun 2023','January to June 2023') then 1
							when cfm.SubmissionPeriod = 'July to December 2023' then 2
							when cfm.SubmissionPeriod in ('Jan to Jun 2024','January to June 2024') then 3
							when cfm.SubmissionPeriod in ('January to December 2025') then 4
							when cfm.SubmissionPeriod in ('Jan to Jun 2025','January to June 2025') then 5
							when cfm.SubmissionPeriod in ('January to December 2026','July to December 2025') then 6
							when cfm.SubmissionPeriod in ('Jan to Jun 2026','January to June 2026') then 7
							when cfm.SubmissionPeriod = 'July to December 2026' then 8
							when cfm.SubmissionPeriod in ('Jan to Jun 2027','January to June 2027') then 9
							when cfm.SubmissionPeriod = 'July to December 2027' then 10
							when cfm.SubmissionPeriod in ('Jan to Jun 2028','January to June 2028') then 11
							when cfm.SubmissionPeriod = 'July to December 2028' then 12
							else 0
							end as SubmissionPeriod
					, case when cfm.SubmissionPeriod in ('Jan to Jun 2023','January to June 2023','July to December 2023') then 2023
							when cfm.SubmissionPeriod in ('Jan to Jun 2024','January to June 2024','January to December 2025') then 2024
							when cfm.SubmissionPeriod in ('Jan to Jun 2025','January to June 2025','January to December 2026','July to December 2025') then 2025
							when cfm.SubmissionPeriod in ('Jan to Jun 2026','January to June 2026','July to December 2026') then 2026
							when cfm.SubmissionPeriod in ('Jan to Jun 2027','January to June 2027','July to December 2027') then 2027
							when cfm.SubmissionPeriod in ('Jan to Jun 2028','January to June 2028','July to December 2028') then 2028
							else 0
							end as ReportingYear
					, CONVERT(DATETIME,substring(cfm.Created,1,23)) as Submission_time
					, cs.id as ComplianceSchemeId
					, CAST(cfm.FileName AS NVARCHAR(256)) as FileName --CF032: every column on CompanyDetails/Organisations/Persons/ComplianceSchemes/cosmos_file_metadata is declared NVARCHAR(8000) (16000 bytes) regardless of actual content - any CASE/ISNULL built from them inherits that same massive declared width, which was forcing an 8060-byte worktable in the final SELECT even with zero explicit GROUP BY/ORDER BY/DISTINCT. Explicit CAST here narrows the type at the source so it propagates through every downstream CTE
					, 'Processed' as File_Status
					, 'CD table' as Source
					, CAST(p.FirstName AS NVARCHAR(256)) as FirstName --CF032
					, CAST(cs.Name AS NVARCHAR(256)) as 'CS_Name' --CF032
					, N.Name as 'CS Nation' --Nations.Name is NVARCHAR(108) - already narrow, no cast needed
					, case when cs.id is NULL then 'DP' else 'CS' end as 'Who submitted'
					, CAST(cd.FileName AS NVARCHAR(256)) as cd_filename --CF032
					, case upper(trim(ISNULL(CAST(fs.Regulator_Status AS NVARCHAR(50)),'PENDING')))
						when 'QUERIED' then 'PENDING'
						when 'GRANTED' then 'ACCEPTED'
						when 'REFUSED' then 'ACCEPTED'
						when 'CANCELLED' then 'ACCEPTED'
						when 'APPROVED' then 'ACCEPTED'
						else upper(trim(ISNULL(CAST(fs.Regulator_Status AS NVARCHAR(50)),'PENDING'))) end as Regulator_Status --CF032
					, upper(trim(ISNULL(CAST(fs.Regulator_Status AS NVARCHAR(50)),'PENDING'))) as Actual_Regulator_Status --CF032
					, case when cd.subsidiary_id is null then CAST(cd.organisation_size AS NVARCHAR(10))
					     else null end as cd_organisation_size--YM008 / CF032
					--,cd.organisation_size as cd_organisation_size
					, '202X-P0'as cd_submission_period_code --YM001
					, fs.IsResubmission_identifier--YM006
			from [rpd].[CompanyDetails] cd
			left join rpd.Organisations o on o.ReferenceNumber = cd.organisation_id
			left join [rpd].[cosmos_file_metadata] cfm on cfm.FileName = cd.FileName
			left join [rpd].[ComplianceSchemes] cs on cs.ExternalId = cfm.ComplianceSchemeId
			left join rpd.users u on u.USerId = cfm.UserId
			left join rpd.persons p on p.UserId = u.id
			left join rpd.Nations N on N.Id = cs.NationId
			left join [dbo].[v_submitted_pom_org_file_status] fs on fs.cfm_FileId = cfm.FileId and fs.FileType = 'CompanyDetails' --CF016: match Paycal's join key (fileid+filetype) instead of FileName - v_submitted_pom_org_file_status dedupes by cfm_FileId, so joining by FileName could land on an unrelated row whenever cfm.FileId is null for the real file
			where cd.organisation_size = 'L' --CF022: client only uses Large producer data - prune early for performance. Also naturally keeps just the parent CompanyDetails row (subsidiary rows have organisation_size blanked out), which is all this org-level cte needs anyway
			and cfm.SubmissionPeriod in (select SP from ValidSubmissionPeriodsForTarget) --CF023: client only needs TargetObligationYear's cycle - single source of truth, see ValidSubmissionPeriodsForTarget above
		) A
),
ORG_LATEST_ACCEPTED as --CF016: latest ACCEPTED-only registration (Paycal-compatible: sp_GetPaycalPomData's latest_accepted_registration), independent of the first/QUERIED/rejected-fallback handling below
(
	select *
		, row_number() over(partition by OrganisationId, ReferenceNumber, SubmissionPeriod order by Submission_time desc, Source asc, cd_organisation_size desc) as Latest_accepted_submission
	from ORG
	where Actual_Regulator_Status in ('GRANTED','ACCEPTED')
),
ORG_REJECTED_SUBMISSION_ONLY as --YM006
(
	select *
		, row_number() over(partition by OrganisationId, ReferenceNumber, SubmissionPeriod order by Submission_time asc, Source asc,cd_organisation_size desc) as First_rejected_submission
		, row_number() over(partition by OrganisationId, ReferenceNumber, SubmissionPeriod order by Submission_time desc, Source asc,cd_organisation_size desc) as Last_rejected_submission
	from ORG
	where Regulator_Status = 'REJECTED' and IsResubmission_identifier=0
),
ORG_REJECTED_RESUBMISSION_ONLY as --YM006
(
	select *
		, row_number() over(partition by OrganisationId, ReferenceNumber, SubmissionPeriod order by Submission_time asc, Source asc,cd_organisation_size desc) as First_rejected_resubmission
		, row_number() over(partition by OrganisationId, ReferenceNumber, SubmissionPeriod order by Submission_time desc, Source asc,cd_organisation_size desc) as Last_rejected_resubmission
	from ORG
	where Regulator_Status = 'REJECTED' and IsResubmission_identifier=1
),
ORG_PENDING_ACCEPTED_RESUBMISSION_ONLY as --YM006
(
	select DISTINCT *
		, row_number() over(partition by OrganisationId, ReferenceNumber, SubmissionPeriod order by Submission_time asc, Source asc,cd_organisation_size desc) as First_pending_accepted_resubmission
		, row_number() over(partition by OrganisationId, ReferenceNumber, SubmissionPeriod order by Submission_time desc, Source asc,cd_organisation_size desc) as Last_pending_accepted_resubmission
	from ORG
	where (Regulator_Status = 'PENDING' or  Regulator_Status = 'ACCEPTED')  and IsResubmission_identifier=1
),
ORG_PENDING_ACCEPT_ONLY as
(
	select *
		, row_number() over(partition by OrganisationId, ReferenceNumber, SubmissionPeriod order by Submission_time asc, Source asc,cd_organisation_size desc) as First_pending_accepted_submission
		, row_number() over(partition by OrganisationId, ReferenceNumber, SubmissionPeriod order by Submission_time desc, Source asc,cd_organisation_size desc) as Last_pending_accepted_submission
	from ORG
	where (Regulator_Status = 'PENDING' or  Regulator_Status = 'ACCEPTED') and IsResubmission_identifier=0
),

/** YM003 : Logic change for First and Latest Registration File Submissions for status queried **/
ORG_QUERIED as
(select * from ORG_PENDING_ACCEPT_ONLY where Actual_Regulator_Status = 'QUERIED' ),

ORG_LATEST_IS_NOT_QUERIED as --YM003
(
select distinct pa.OrganisationId, pa.ReferenceNumber, pa.SubmissionPeriod
from ORG_PENDING_ACCEPT_ONLY pa
inner join ORG_QUERIED oq on oq.OrganisationId = pa.OrganisationId and oq.ReferenceNumber = pa.ReferenceNumber and oq.SubmissionPeriod = pa.SubmissionPeriod
where pa.Last_pending_accepted_submission = 1
and pa.Actual_Regulator_Status <> 'QUERIED'
),

ORG_PENDING_ACCEPT_ONLY_UPDATED as --YM003
(
select OPA.* from ORG_PENDING_ACCEPT_ONLY OPA
left join ORG_LATEST_IS_NOT_QUERIED ONQ on OPA.OrganisationId = ONQ.OrganisationId and OPA.ReferenceNumber = ONQ.ReferenceNumber and OPA.SubmissionPeriod = ONQ.SubmissionPeriod
where ONQ.OrganisationId is null
		or
		(OPA.Actual_Regulator_Status <> 'QUERIED' and  ONQ.OrganisationId is not null)
),

ORG_PENDING_ACCEPT_ONLY_UPDATED_WITH_LEAD as --YM005
(
	select *
		, lead(Actual_Regulator_Status,1,NULL) over (partition by OrganisationId,	ReferenceNumber,	SubmissionPeriod order by Submission_time asc) as lead_Actual_Regulator_Status
		, lead(FileName,1,NULL) over (partition by OrganisationId,	ReferenceNumber,	SubmissionPeriod order by Submission_time asc) as lead_FileName
	from ORG_PENDING_ACCEPT_ONLY_UPDATED
),

ORG_PENDING_ACCEPT_ONLY_UPDATED_WITH_LEAD_DUPLICATE_QUERIED_REMOVED as --YM005
(
	select * from ORG_PENDING_ACCEPT_ONLY_UPDATED_WITH_LEAD
	except(
		select *
		from ORG_PENDING_ACCEPT_ONLY_UPDATED_WITH_LEAD
		where Actual_Regulator_Status = 'QUERIED' and lead_Actual_Regulator_Status = 'QUERIED' and FileName <> lead_FileName
		)
),

ORG_PENDING_ACCEPT_ONLY_UPDATED_WITH_LEAD_DUPLICATE_QUERIED_REMOVED_WITH_RANK as --YM005
(
	select *
		, row_number() over(partition by OrganisationId, ReferenceNumber, SubmissionPeriod order by Submission_time asc, Source asc,cd_organisation_size desc) as First_pending_accepted_submission_updated
		, row_number() over(partition by OrganisationId, ReferenceNumber, SubmissionPeriod order by Submission_time desc, Source asc,cd_organisation_size desc) as Last_pending_accepted_submission_updated
	from ORG_PENDING_ACCEPT_ONLY_UPDATED_WITH_LEAD_DUPLICATE_QUERIED_REMOVED
),

ORG_REJECTED_WITH_OUT_PENDING_ACCEPTED as --YM003
(
	select rej.*
	from ORG_REJECTED_SUBMISSION_ONLY rej
	left join ORG_PENDING_ACCEPT_ONLY_UPDATED_WITH_LEAD_DUPLICATE_QUERIED_REMOVED_WITH_RANK pa on pa.OrganisationId = rej.OrganisationId and pa.ReferenceNumber = rej.ReferenceNumber and pa.SubmissionPeriod = rej.SubmissionPeriod
	where pa.OrganisationId is null
),
ORG_REJECTED_WITH_OUT_PENDING_ACCEPTED_RESUB as --YM006
(
	select rej.* from ORG_REJECTED_RESUBMISSION_ONLY rej
	left join ORG_PENDING_ACCEPTED_RESUBMISSION_ONLY par
	on par.OrganisationId = rej.OrganisationId and par.ReferenceNumber = rej.ReferenceNumber and par.SubmissionPeriod = rej.SubmissionPeriod
	where rej.Actual_Regulator_Status ='Rejected' and par.OrganisationId is null
),
f_org_sql as
 (
	select ReferenceNumber as 'Org ID', SubmissionPeriod as 'Rank', ReportingYear, Submission_time as 'Submission date time', case when ComplianceSchemeId is null then 'DP' else CS_Name end as 'Submitted by',	File_Status as 'Submission status', Regulator_Status as 'Regulator Decision', Actual_Regulator_Status as 'Actual Regulator Decision',	[Who submitted], [CS Nation] , cd_filename, ComplianceSchemeId, cd_organisation_size,cd_submission_period_code ,IsResubmission_identifier
	from ORG_PENDING_ACCEPT_ONLY_UPDATED_WITH_LEAD_DUPLICATE_QUERIED_REMOVED_WITH_RANK --YM001--YM003 --YM005--YM006
	where First_pending_accepted_submission_updated = 1
	union
	select ReferenceNumber as 'Org ID', SubmissionPeriod as 'Rank', ReportingYear, Submission_time as 'Submission date time', case when ComplianceSchemeId is null then 'DP' else CS_Name end as 'Submitted by',	File_Status as 'Submission status', Regulator_Status as 'Regulator Decision', Actual_Regulator_Status as 'Actual Regulator Decision',	[Who submitted], [CS Nation] , cd_filename, ComplianceSchemeId, cd_organisation_size,cd_submission_period_code ,IsResubmission_identifier--YM006
	from ORG_REJECTED_WITH_OUT_PENDING_ACCEPTED
	where Last_rejected_submission = 1
 ) ,
 l_org_sql as --CF016: latest ACCEPTED-only registration, replacing the old "latest regardless of status" + QUERIED-dedup + rejected-fallback chain above (still used by f_org_sql for "first")
 (
	select ReferenceNumber as 'Org ID', SubmissionPeriod as 'Rank', ReportingYear, Submission_time as 'Submission date time', case when ComplianceSchemeId is null then 'DP' else CS_Name end as 'Submitted by',	File_Status as 'Submission status', Regulator_Status as 'Regulator Decision', Actual_Regulator_Status as 'Actual Regulator Decision',	[Who submitted], [CS Nation] , cd_filename, ComplianceSchemeId, cd_organisation_size,cd_submission_period_code ,IsResubmission_identifier
	from ORG_LATEST_ACCEPTED
	where Latest_accepted_submission = 1
 ),

POM_submitted_file_list as --CF020: mirrors v_submitted_pom_org_file_status's submitted_file_list. POM's status is computed inline here (instead of joining that view) because its null-FileId backfill (sets A/B/C) is entirely registration-only (gated on SubmissionType='Registration'/type='RegulatorRegistrationDecision') and is otherwise paid for on every query regardless of FileType - see history note above
(
	select distinct SubmissionId, FileId, CONVERT(DATETIME,substring(Created,1,23)) as file_submitted_ts
	from rpd.SubmissionEvents
	where Type in ('Submitted')
	and Fileid is not null
	and (IsResubmission is null or IsResubmission = 0)
),
POM_first_PackagingResubmissionReferenceNumberCreated as --CF020: mirrors first_PackagingResubmissionReferenceNumberCreated_entry
(
	select distinct SubmissionId, min(CONVERT(DATETIME,substring(Created,1,23))) as first_ts
	from rpd.SubmissionEvents
	where Type in ('PackagingResubmissionReferenceNumberCreated')
	group by SubmissionId
),
POM_resubmitted_list as --CF020: mirrors resubmitted_POM_list
(
	select distinct SubmissionId, FileId, IsResubmission as POM_resubmission_identifier
	from rpd.SubmissionEvents
	where Type in ('Submitted')
	and Fileid is not null
	and IsResubmission = 1
	union
	select sfl.SubmissionId, sfl.FileId, 1 as POM_resubmission_identifier
	from POM_submitted_file_list sfl
	inner join POM_first_PackagingResubmissionReferenceNumberCreated ent
		on sfl.SubmissionId = ent.SubmissionId
		and sfl.file_submitted_ts >= ent.first_ts
),
POM_resubmission_ids as --CF020: mirrors resubmission_ids (general - not Pom-specific in the source view either, kept equivalent here)
(
	select IsResubmission_identifier, Fileid
	from
	(
		select distinct ISNULL(IsResubmission,0) as IsResubmission_identifier, Fileid
			, row_number() over(partition by Fileid order by CONVERT(DATETIME,substring(Created,1,23))) as rnk
		from rpd.SubmissionEvents
		where Type = 'Submitted'
		and Fileid is not null
	) A
	where rnk = 1
),
POM_submitted_Fileids as --CF020: mirrors submitted_Fileids
(
	select distinct Fileid as submitted_Fileid, SubmissionId as SubmissionId_of_submitted_record, CONVERT(DATETIME,substring(Created,1,23)) as Submitted_ts
	from rpd.SubmissionEvents
	where Type = 'Submitted' and Fileid is not null
),
POM_app_submitted_candidates as --CF020: mirrors get_all_RegistrationApplicationSubmitted
(
	select sub.submitted_Fileid as app_submitted_Fileid
		, app_sub.SubmissionId, app_sub.SubmissionEventId
		, row_number() over(partition by app_sub.SubmissionId, app_sub.SubmissionEventId order by sub.Submitted_ts desc) as RN
	from rpd.SubmissionEvents app_sub
	inner join POM_submitted_Fileids sub
		on sub.SubmissionId_of_submitted_record = app_sub.SubmissionId
		and CONVERT(DATETIME,substring(app_sub.Created,1,23)) >= sub.Submitted_ts
	where app_sub.Type in ('RegistrationApplicationSubmitted','PackagingResubmissionApplicationSubmitted')
),
POM_app_submitted as --CF020: mirrors top_matching_og_get_all_RegistrationApplicationSubmitted
(
	select distinct app_submitted_Fileid from POM_app_submitted_candidates where RN = 1
),
POM_decision_raw as --CF020: mirrors the Pom branch of v_submitted_pom_org_file_status's res CTE, minus the registration-only fileid backfill - Pom decisions always carry their own FileId directly, so no backfill is needed
(
	select distinct cfm.FileId
		, cfm.Created
		, se.Created as Decision_Date
		, case
			when ISNULL(rpl.POM_resubmission_identifier,0) = 1 and app_submitted.app_submitted_Fileid is not null and se.Decision is null then 'Pending'
			when ISNULL(rpl.POM_resubmission_identifier,0) = 1 and app_submitted.app_submitted_Fileid is null and se.Decision is null then 'Uploaded'
			when ISNULL(rpl.POM_resubmission_identifier,0) = 0 and se.Decision is null then 'Pending'
			else se.Decision
			end as Regulator_Status
		, ISNULL(rid.IsResubmission_identifier,0) as IsResubmission_identifier
	from rpd.cosmos_file_metadata cfm
	left join rpd.SubmissionEvents se on se.FileId = cfm.FileId and se.Type = 'RegulatorPoMDecision'
	left join POM_resubmission_ids rid on rid.Fileid = cfm.FileId
	left join POM_resubmitted_list rpl on rpl.FileId = cfm.FileId
	left join POM_app_submitted app_submitted on app_submitted.app_submitted_Fileid = cfm.FileId
	where cfm.FileType = 'Pom'
	and cfm.SubmissionPeriod in (select SP from ValidSubmissionPeriodsForTarget) --CF023: same TargetObligationYear-cycle prune as the POM cte below
),
POM_decision as --CF020: final per-file dedup, matching v_submitted_pom_org_file_status's row_number()-over-cfm_FileId pattern
(
	select FileId, Regulator_Status, IsResubmission_identifier
	from
	(
		select *, row_number() over(partition by FileId order by Decision_Date desc, Created desc) as RowNumber
		from POM_decision_raw
	) ranked
	where RowNumber = 1
),
POM as
(
		select *
			, row_number() over(partition by OrganisationId, ReferenceNumber, SubmissionPeriod order by Submission_time asc, Source desc) as First_submission
			, row_number() over(partition by OrganisationId, ReferenceNumber, SubmissionPeriod order by Submission_time desc, Source desc) as Last_submission
		from
		(
			select distinct o.id as OrganisationId, pm.organisation_id as ReferenceNumber
					, case when cfm.SubmissionPeriod in ('Jan to Jun 2023','January to June 2023') then 1
							when cfm.SubmissionPeriod = 'July to December 2023' then 2
							when cfm.SubmissionPeriod in ('Jan to Jun 2024','January to June 2024') then 3
							when cfm.SubmissionPeriod = 'July to December 2024' then 4
							when cfm.SubmissionPeriod in ('Jan to Jun 2025','January to June 2025') then 5
							when cfm.SubmissionPeriod = 'July to December 2025' then 6
							when cfm.SubmissionPeriod in ('Jan to Jun 2026','January to June 2026') then 7
							when cfm.SubmissionPeriod = 'July to December 2026' then 8
							when cfm.SubmissionPeriod in ('Jan to Jun 2027','January to June 2027') then 9
							when cfm.SubmissionPeriod = 'July to December 2027' then 10
							when cfm.SubmissionPeriod in ('Jan to Jun 2028','January to June 2028') then 11
							when cfm.SubmissionPeriod = 'July to December 2028' then 12
							else 0
							end as SubmissionPeriod
					, case when cfm.SubmissionPeriod in ('Jan to Jun 2023','January to June 2023','July to December 2023') then 2023
							when cfm.SubmissionPeriod in ('Jan to Jun 2024','January to June 2024','July to December 2024') then 2024
							when cfm.SubmissionPeriod in ('Jan to Jun 2025','January to June 2025','July to December 2025') then 2025
							when cfm.SubmissionPeriod in ('Jan to Jun 2026','January to June 2026','July to December 2026') then 2026
							when cfm.SubmissionPeriod in ('Jan to Jun 2027','January to June 2027','July to December 2027') then 2027
							when cfm.SubmissionPeriod in ('Jan to Jun 2028','January to June 2028','July to December 2028') then 2028
							else 0
							end as ReportingYear
					, CONVERT(DATETIME,substring(cfm.Created,1,23)) as Submission_time
					, cs.id as ComplianceSchemeId
					, CAST(cfm.FileName AS NVARCHAR(256)) as FileName --CF032: every column on Pom/Organisations/Persons/ComplianceSchemes/cosmos_file_metadata is declared NVARCHAR(8000) (16000 bytes) regardless of actual content - any CASE/ISNULL built from them inherits that same massive declared width, which was forcing an 8060-byte worktable in the final SELECT even with zero explicit GROUP BY/ORDER BY/DISTINCT. Explicit CAST here narrows the type at the source so it propagates through every downstream CTE
					, 'Processed' as File_Status
					, 'POM table' as Source
					, CAST(p.FirstName AS NVARCHAR(256)) as FirstName --CF032
					, CAST(cs.Name AS NVARCHAR(256)) as 'CS_Name' --CF032
					, N.Name as 'CS Nation' --Nations.Name is NVARCHAR(108) - already narrow, no cast needed
					, case when cs.id is NULL then 'DP' else 'CS' end as 'Who submitted'
					, CAST(pm.FileName AS NVARCHAR(256)) as pm_filename --CF032
					, case upper(trim(ISNULL(CAST(fs.Regulator_Status AS NVARCHAR(50)),'PENDING')))
						when 'QUERIED' then 'PENDING'
						when 'GRANTED' then 'ACCEPTED'
						when 'REFUSED' then 'ACCEPTED'
						when 'CANCELLED' then 'ACCEPTED'
						when 'APPROVED' then 'ACCEPTED'
						else upper(trim(ISNULL(CAST(fs.Regulator_Status AS NVARCHAR(50)),'PENDING'))) end as Regulator_Status --CF032
					, upper(trim(ISNULL(CAST(fs.Regulator_Status AS NVARCHAR(50)),'PENDING'))) as Actual_Regulator_Status --CF032
					, CAST(pm.organisation_size AS NVARCHAR(10)) as pm_organisation_size --CF032
					, CAST(pm.submission_period AS NVARCHAR(50)) as pm_submission_period_code --YM001 / CF032
					, fs.IsResubmission_identifier --YM006
			from [rpd].[Pom] pm
			left join rpd.Organisations o on o.ReferenceNumber = pm.organisation_id
			left join [rpd].[cosmos_file_metadata] cfm on cfm.FileName = pm.FileName
			left join [rpd].[ComplianceSchemes] cs on cs.ExternalId = cfm.ComplianceSchemeId
			left join rpd.users u on u.USerId = cfm.UserId
			left join rpd.persons p on p.UserId = u.id
			left join rpd.Nations N on N.Id = cs.NationId
			left join POM_decision fs on fs.FileId = cfm.FileId --CF020: Pom-only status computed inline above, instead of joining v_submitted_pom_org_file_status
			where fs.Regulator_Status <> 'Uploaded' --YM007
			and pm.organisation_size = 'L' --CF022: client only uses Large producer data - prune early for performance
			and cfm.SubmissionPeriod in (select SP from ValidSubmissionPeriodsForTarget) --CF023: client only needs TargetObligationYear's cycle - single source of truth, see ValidSubmissionPeriodsForTarget above
		) A
),
POM_LATEST_ACCEPTED as --CF016: latest ACCEPTED-only POM submission (Paycal-compatible: sp_GetPaycalPomData's latest_accepted_pom)
(
	select *
		, row_number() over(partition by OrganisationId, ReferenceNumber, SubmissionPeriod order by Submission_time desc, Source desc) as Latest_accepted_submission
	from POM
	where Actual_Regulator_Status = 'ACCEPTED'
),
POM_REJECTED_ONLY as
(
	select *
		, row_number() over(partition by OrganisationId, ReferenceNumber, SubmissionPeriod order by Submission_time asc, Source asc) as First_rejected_submission
		, row_number() over(partition by OrganisationId, ReferenceNumber, SubmissionPeriod order by Submission_time desc, Source asc) as Last_rejected_submission
	from POM
	where Regulator_Status = 'REJECTED'
),
POM_PENDING_ACCEPT_ONLY as
(
	select *
		, row_number() over(partition by OrganisationId, ReferenceNumber, SubmissionPeriod order by Submission_time asc, Source asc) as First_pending_accepted_submission
		, row_number() over(partition by OrganisationId, ReferenceNumber, SubmissionPeriod order by Submission_time desc, Source asc) as Last_pending_accepted_submission
	from POM
	where (Regulator_Status = 'PENDING' or  Regulator_Status = 'ACCEPTED')
),
POM_REJECTED_WITH_OUT_PENDING_ACCEPTED as
(
	select rej.*
	from POM_REJECTED_ONLY rej
	left join POM_PENDING_ACCEPT_ONLY pa on pa.OrganisationId = rej.OrganisationId and pa.ReferenceNumber = rej.ReferenceNumber and pa.SubmissionPeriod = rej.SubmissionPeriod
	where pa.OrganisationId is null
),
f_pom_sql as
 (
	select ReferenceNumber as 'Org ID', SubmissionPeriod as 'Rank', ReportingYear, Submission_time as 'Submission date time', case when ComplianceSchemeId is null then 'DP' else CS_Name end as 'Submitted by',	File_Status as 'Submission status', Regulator_Status as 'Regulator Decision', Actual_Regulator_Status as 'Actual Regulator Decision',	[Who submitted], [CS Nation] , pm_filename, ComplianceSchemeId, pm_organisation_size,pm_submission_period_code ,IsResubmission_identifier --YM001
	from POM_PENDING_ACCEPT_ONLY
	where First_pending_accepted_submission = 1
	union all --CF027: was UNION - both branches are already mutually exclusive by construction (POM_REJECTED_WITH_OUT_PENDING_ACCEPTED explicitly excludes any Org/Ref/Period that has a row in POM_PENDING_ACCEPT_ONLY), so the DISTINCT was pure overhead; that dedup sort over ~14 wide columns was forcing a worktable past the 8060-byte limit once this CTE got pulled into the new Winning_POM_FileNames join below (CTEs inline, so narrowing a wrapper CTE doesn't skip this UNION's own work)
	select ReferenceNumber as 'Org ID', SubmissionPeriod as 'Rank', ReportingYear, Submission_time as 'Submission date time', case when ComplianceSchemeId is null then 'DP' else CS_Name end as 'Submitted by',	File_Status as 'Submission status', Regulator_Status as 'Regulator Decision', Actual_Regulator_Status as 'Actual Regulator Decision',	[Who submitted], [CS Nation] , pm_filename, ComplianceSchemeId, pm_organisation_size,pm_submission_period_code ,IsResubmission_identifier--YM001
	from POM_REJECTED_WITH_OUT_PENDING_ACCEPTED
	where Last_rejected_submission = 1
 ),
l_pom_sql as --CF016: latest ACCEPTED-only POM submission, replacing "latest among pending+accepted regardless of status" (still used by f_pom_sql for "first")
 (
	select ReferenceNumber as 'Org ID', SubmissionPeriod as 'Rank', ReportingYear, Submission_time as 'Submission date time', case when ComplianceSchemeId is null then 'DP' else CS_Name end as 'Submitted by',	File_Status as 'Submission status', Regulator_Status as 'Regulator Decision', Actual_Regulator_Status as 'Actual Regulator Decision',	[Who submitted], [CS Nation] , pm_filename, ComplianceSchemeId, pm_organisation_size,pm_submission_period_code ,IsResubmission_identifier--YM001
	from POM_LATEST_ACCEPTED
	where Latest_accepted_submission = 1
 ),
Rank_On_CS_Submission_for_org_file as
(
	select *
		, row_number() over(partition by ComplianceSchemeId, FileType, SubmissionPeriod order by Submission_time desc) as Rank_on_submission_timestamp
	from
	(
		select cfm.OrganisationId,  cfm.FileType
								, case when cfm.SubmissionPeriod in ('Jan to Jun 2023','January to June 2023') then 1
										when cfm.SubmissionPeriod = 'July to December 2023' then 2
										when cfm.SubmissionPeriod in ('Jan to Jun 2024','January to June 2024') then 3
										when cfm.SubmissionPeriod in ('January to December 2025') then 4
										when cfm.SubmissionPeriod in ('Jan to Jun 2025','January to June 2025') then 5
										when cfm.SubmissionPeriod = 'July to December 2025' then 6
										when cfm.SubmissionPeriod in ('Jan to Jun 2026','January to June 2026') then 7
										when cfm.SubmissionPeriod = 'July to December 2026' then 8
										when cfm.SubmissionPeriod in ('Jan to Jun 2027','January to June 2027') then 9
										when cfm.SubmissionPeriod = 'July to December 2027' then 10
										when cfm.SubmissionPeriod in ('Jan to Jun 2028','January to June 2028') then 11
										when cfm.SubmissionPeriod = 'July to December 2028' then 12
										else 0
										end as SubmissionPeriod
								, case when cfm.SubmissionPeriod in ('Jan to Jun 2023','January to June 2023','July to December 2023') then 2023
										when cfm.SubmissionPeriod in ('Jan to Jun 2024','January to June 2024','January to December 2025') then 2024
										when cfm.SubmissionPeriod in ('Jan to Jun 2025','January to June 2025','July to December 2025') then 2025
										when cfm.SubmissionPeriod in ('Jan to Jun 2026','January to June 2026','July to December 2026') then 2026
										when cfm.SubmissionPeriod in ('Jan to Jun 2027','January to June 2027','July to December 2027') then 2027
										when cfm.SubmissionPeriod in ('Jan to Jun 2028','January to June 2028','July to December 2028') then 2028
										else 0
										end as ReportingYear
								, cfm.ComplianceSchemeId
		, CONVERT(DATETIME,substring(cfm.Created,1,23)) as Submission_time
		, cfm.FileName
		From [rpd].[cosmos_file_metadata] cfm
		left join [rpd].[error_files_not_processed] ef on ef.FileName = cfm.FileName
		where cfm.FileType in ('CompanyDetails')
		and cfm.ComplianceSchemeId is not null
		and cfm.SubmissionPeriod in (select SP from ValidSubmissionPeriodsForTarget) --CF023: previously unscoped - was joining/ranking every CompanyDetails-by-CS file ever submitted, regardless of year, even though only TargetObligationYear's cycle can ever surface in the final output via the RankId join below
	) A
),

Latest_org_by_CS as
(
	select distinct CD.organisation_id, cs.id as ComplianceSchemeId, SubmissionPeriod, 'Y' as Is_present_latest_cs_sub_org
	from Rank_On_CS_Submission_for_org_file RS
	inner join [rpd].[CompanyDetails] CD on RS.FileName = CD.FileName
	inner join [rpd].[ComplianceSchemes] cs on cs.ExternalId = RS.ComplianceSchemeId
	where Rank_on_submission_timestamp = 1
	and FileType = 'CompanyDetails'
),

Rank_On_CS_Submission_for_pom_file as
(
	select *
		, row_number() over(partition by ComplianceSchemeId, FileType, SubmissionPeriod order by Submission_time desc) as Rank_on_submission_timestamp
	from
	(
		select cfm.OrganisationId,  cfm.FileType
								, case when cfm.SubmissionPeriod in ('Jan to Jun 2023','January to June 2023') then 1
										when cfm.SubmissionPeriod = 'July to December 2023' then 2
										when cfm.SubmissionPeriod in ('Jan to Jun 2024','January to June 2024') then 3
										when cfm.SubmissionPeriod = 'July to December 2024' then 4
										when cfm.SubmissionPeriod in ('Jan to Jun 2025','January to June 2025') then 5
										when cfm.SubmissionPeriod = 'July to December 2025' then 6
										when cfm.SubmissionPeriod in ('Jan to Jun 2026','January to June 2026') then 7
										when cfm.SubmissionPeriod = 'July to December 2026' then 8
										when cfm.SubmissionPeriod in ('Jan to Jun 2027','January to June 2027') then 9
										when cfm.SubmissionPeriod = 'July to December 2027' then 10
										when cfm.SubmissionPeriod in ('Jan to Jun 2028','January to June 2028') then 11
										when cfm.SubmissionPeriod = 'July to December 2028' then 12
										else 0
										end as SubmissionPeriod
								, case when cfm.SubmissionPeriod in ('Jan to Jun 2023','January to June 2023','July to December 2023') then 2023
										when cfm.SubmissionPeriod in ('Jan to Jun 2024','January to June 2024','July to December 2024') then 2024
										when cfm.SubmissionPeriod in ('Jan to Jun 2025','January to June 2025','July to December 2025') then 2025
										when cfm.SubmissionPeriod in ('Jan to Jun 2026','January to June 2026','July to December 2026') then 2026
										when cfm.SubmissionPeriod in ('Jan to Jun 2027','January to June 2027','July to December 2027') then 2027
										when cfm.SubmissionPeriod in ('Jan to Jun 2028','January to June 2028','July to December 2028') then 2028
										else 0
										end as ReportingYear
								, cfm.ComplianceSchemeId
		, CONVERT(DATETIME,substring(cfm.Created,1,23)) as Submission_time
		, cfm.FileName
		From [rpd].[cosmos_file_metadata] cfm
		left join [rpd].[error_files_not_processed] ef on ef.FileName = cfm.FileName
		where cfm.FileType in ('Pom')
		and cfm.ComplianceSchemeId is not null
		and cfm.SubmissionPeriod in (select SP from ValidSubmissionPeriodsForTarget) --CF023: previously unscoped - was joining/ranking every Pom-by-CS file ever submitted, regardless of year, even though only TargetObligationYear's cycle can ever surface in the final output via the RankId join below
	) A
),

Latest_pom_by_CS as
(
	select distinct PM.organisation_id, cs.id as ComplianceSchemeId, SubmissionPeriod, 'Y' as Is_present_latest_cs_sub_pom
	from Rank_On_CS_Submission_for_pom_file RS
	inner join [rpd].[Pom] PM on RS.FileName = PM.FileName
	inner join [rpd].[ComplianceSchemes] cs on cs.ExternalId = RS.ComplianceSchemeId
	where Rank_on_submission_timestamp = 1
	and FileType = 'Pom'
),
--YM006
/*
rptRegistrationRegistered as
(
	select distinct organisation_id, 'Y' as Is_Present_in_Reg_report
	from [dbo].[registration]
),
rptPOM_All_Submissions as
(
	select distinct OrganisationID as organisation_id, 'Y' as Is_Present_in_POM_report
	from [dbo].[v_POM_All_Submissions]
	where OrganisationID is not null
),
*/
 enr as
 (
		select pocon.OrganisationId, ES.[Name] , CONVERT(DATETIME,substring(E.CreatedOn,1,23)) 'Enrolment_date_time', CONVERT(DATETIME,substring(E.LastUpdatedOn,1,23)) 'Enrolment_status_date_time',
		row_number() over(partition by pocon.OrganisationId order by ES.Name) as RN
		From rpd.PersonOrganisationConnections pocon
		inner join rpd.Enrolments E on E.ConnectionId = pocon.Id
		inner join rpd.ServiceRoles SR on SR.Id = E.ServiceRoleId
		inner join rpd.EnrolmentStatuses ES on ES.Id = E.EnrolmentStatusId
		where SR.[Key] = 'Packaging.ApprovedPerson'
 ),

 CSN as
 (
	 select O.Id as OrganisationId, N.Name as CS_Nation
	from rpd.Organisations O
	inner join [rpd].[OrganisationsConnections] OC
		on OC.FromOrganisationId = O.Id
	inner join [rpd].[SelectedSchemes] SS
		on SS.OrganisationConnectionId = OC.Id
	inner join [rpd].[ComplianceSchemes] CS
		on SS.ComplianceSchemeId = CS.Id
	inner join rpd.Nations N on N.Id = CS.NationId
	where O.IsComplianceScheme = 0 and OC.IsDeleted = 0 and SS.IsDeleted = 0
 ),
base_sql as
(
	select --Org.Id,
		TwoRow.RankId,
		TwoRow.SP,
		TwoRow.Reporting_Year,
		Org.ReferenceNumber as 'Org ID',
		Org.Name as 'Org Name',
		ISNULL(Org.CompaniesHouseNumber,'') as CH,
		N.Name as 'Nation of Enrolment',
		enr.Enrolment_date_time,
		enr.[Name] as 'Status of enrolment',
		enr.Enrolment_status_date_time,
		ISNULL(CSN.CS_Nation,'') as 'Nation of Compliance Scheme regulator',
		Org.IsDeleted as 'Org soft deleted?'
	from rpd.Organisations Org
	inner join TwoRow on 1 = 1
	left join rpd.Nations N on N.Id = Org.NationId
	left join enr on enr.OrganisationId = org.Id and enr.RN = 1
	left join CSN on CSN.OrganisationId = org.Id
	where Org.IsComplianceScheme = 0
),
submission_count as
(
	select [Org ID],ReportingYear, count(1) as cnt
	from
	(
		select [Org ID],ReportingYear from l_org_sql
		union all
		select [Org ID],ReportingYear From l_pom_sql
	) A
	group by [Org ID],ReportingYear
),
POM_Submission_Narrow as --CF029: standalone, narrow re-derivation of each qualifying POM file's org/period/status/timestamp, used ONLY to compute Winning_POM_FileNames below - does NOT route through POM/f_pom_sql/l_pom_sql, because those still inline back through POM's wide "select distinct" (subquery A, ~18 columns incl. FirstName/CS_Name/CS Nation joins) every time they're referenced, which kept forcing an 8060-byte worktable even after narrowing wrapper CTEs and removing the FULL OUTER JOIN. Source/tie-break uses cfm.ComplianceSchemeId directly instead of joining to ComplianceSchemes (cs.id is null <=> cfm.ComplianceSchemeId is null, barring an orphaned ComplianceSchemeId with no matching row - Source is only a same-timestamp tie-breaker, so this is a safe simplification)
(
	select distinct
		  pm.organisation_id as [Org ID]
		, case when cfm.SubmissionPeriod in ('Jan to Jun 2023','January to June 2023') then 1
				when cfm.SubmissionPeriod = 'July to December 2023' then 2
				when cfm.SubmissionPeriod in ('Jan to Jun 2024','January to June 2024') then 3
				when cfm.SubmissionPeriod = 'July to December 2024' then 4
				when cfm.SubmissionPeriod in ('Jan to Jun 2025','January to June 2025') then 5
				when cfm.SubmissionPeriod = 'July to December 2025' then 6
				when cfm.SubmissionPeriod in ('Jan to Jun 2026','January to June 2026') then 7
				when cfm.SubmissionPeriod = 'July to December 2026' then 8
				when cfm.SubmissionPeriod in ('Jan to Jun 2027','January to June 2027') then 9
				when cfm.SubmissionPeriod = 'July to December 2027' then 10
				when cfm.SubmissionPeriod in ('Jan to Jun 2028','January to June 2028') then 11
				when cfm.SubmissionPeriod = 'July to December 2028' then 12
				else 0
				end as [Rank]
		, CAST(cfm.FileName AS NVARCHAR(256)) as pm_filename --CF032: Pom/cosmos_file_metadata columns are declared NVARCHAR(8000) (16000 bytes) regardless of content - narrow at the source so it doesn't propagate
		, CONVERT(DATETIME,substring(cfm.Created,1,23)) as Submission_time
		, case when cfm.ComplianceSchemeId is NULL then 'DP' else 'CS' end as Source
		, case upper(trim(ISNULL(CAST(pd.Regulator_Status AS NVARCHAR(50)),'PENDING')))
			when 'QUERIED' then 'PENDING'
			when 'GRANTED' then 'ACCEPTED'
			when 'REFUSED' then 'ACCEPTED'
			when 'CANCELLED' then 'ACCEPTED'
			when 'APPROVED' then 'ACCEPTED'
			else upper(trim(ISNULL(CAST(pd.Regulator_Status AS NVARCHAR(50)),'PENDING'))) end as Regulator_Status --CF032
	from [rpd].[Pom] pm
	left join [rpd].[cosmos_file_metadata] cfm on cfm.FileName = pm.FileName
	left join POM_decision pd on pd.FileId = cfm.FileId
	where pd.Regulator_Status <> 'Uploaded' --YM007, matches POM/subquery A's where clause (NULL <> 'Uploaded' is NULL, so a FileId with no POM_decision row is excluded here too - same behaviour as the original)
	and pm.organisation_size = 'L' --CF022
	and cfm.SubmissionPeriod in (select SP from ValidSubmissionPeriodsForTarget) --CF023
),
POM_Narrow_Ranked as
(
	select *
		, row_number() over(partition by [Org ID],[Rank] order by Submission_time asc, Source desc) as First_submission
		, row_number() over(partition by [Org ID],[Rank] order by Submission_time desc, Source desc) as Last_submission
	from POM_Submission_Narrow
),
POM_Narrow_PENDING_ACCEPT_ONLY as
(
	select *
		, row_number() over(partition by [Org ID],[Rank] order by Submission_time asc, Source asc) as First_pending_accepted_submission
	from POM_Narrow_Ranked
	where (Regulator_Status = 'PENDING' or Regulator_Status = 'ACCEPTED')
),
POM_Narrow_REJECTED_ONLY as
(
	select *
		, row_number() over(partition by [Org ID],[Rank] order by Submission_time desc, Source asc) as Last_rejected_submission
	from POM_Narrow_Ranked
	where Regulator_Status = 'REJECTED'
),
POM_Narrow_REJECTED_WITH_OUT_PENDING_ACCEPTED as
(
	select rej.*
	from POM_Narrow_REJECTED_ONLY rej
	left join POM_Narrow_PENDING_ACCEPT_ONLY pa on pa.[Org ID] = rej.[Org ID] and pa.[Rank] = rej.[Rank]
	where pa.[Org ID] is null
),
POM_Narrow_LATEST_ACCEPTED as
(
	select *
		, row_number() over(partition by [Org ID],[Rank] order by Submission_time desc, Source desc) as Latest_accepted_submission
	from POM_Narrow_Ranked
	where Regulator_Status = 'ACCEPTED'
),
f_pom_sql_narrow as --CF029: mirrors f_pom_sql's "first" logic (pending/accepted, else earliest rejected fallback), but computed from the narrow chain above instead of f_pom_sql itself
(
	select [Org ID], [Rank], pm_filename from POM_Narrow_PENDING_ACCEPT_ONLY where First_pending_accepted_submission = 1
	union all
	select [Org ID], [Rank], pm_filename from POM_Narrow_REJECTED_WITH_OUT_PENDING_ACCEPTED where Last_rejected_submission = 1
),
l_pom_sql_narrow as --CF029: mirrors l_pom_sql's "latest accepted" logic, computed from the narrow chain above instead of l_pom_sql itself
(
	select [Org ID], [Rank], pm_filename from POM_Narrow_LATEST_ACCEPTED where Latest_accepted_submission = 1
),
Qualifying_POM_FileNames as --CF019/CF030: the only FileNames the final SELECT ever uses (ISNULL(lps.pm_filename,fps.pm_filename)) - restricting POM_Filtered to just these avoids scanning/joining every POM file ever submitted, only the first/latest-accepted ones actually used in the output. Moved to read from f_pom_sql_narrow/l_pom_sql_narrow (CF030) instead of f_pom_sql/l_pom_sql directly - pm_filename is identical either way, but the originals still route through POM's wide "select distinct" (subquery A) underneath, which kept forcing an 8060-byte worktable here even though only 1 column was ever projected out
(
	select pm_filename from f_pom_sql_narrow where pm_filename is not null
	union
	select pm_filename from l_pom_sql_narrow where pm_filename is not null
),
Winning_POM_FileNames as --CF025/CF031: the SINGLE FileName per (org, period) that the final output actually aggregates from - mirrors agg_POM's own join key (ISNULL(lps.pm_filename,fps.pm_filename)) further down, now ALSO used (CF031) as that join key directly for the 8 PIVOT joins in the final SELECT, instead of each one recomputing ISNULL(lps.pm_filename,fps.pm_filename) inline from the wide f_pom_sql/l_pom_sql - that inline computation, repeated 8 times, kept dragging fps/lps's wide nvarchar columns into the same join region as 300+ PIVOT output columns, forcing an 8060-byte worktable in the final SELECT even after every CTE upstream of it tested clean in isolation. [Rank] kept (not just organisation_id) since it's needed to join back to bs.RankId, matching how f_pom_sql/l_pom_sql are joined today. Qualifying_POM_FileNames above deliberately keeps BOTH first and latest for metadata columns (first/latest submission date etc.), but that's too broad for the Missing-Registration-Data cascade: an earlier "first" submission can still list a subsidiary that a LATER resubmission has since dropped (the same "latest registration supersedes" semantics as the registration side), and checking that superseded file's subsidiaries against current registration data wrongly flags a cascade for the whole org even though the file actually feeding the output never had a problem
(
	select organisation_id, [Rank], pm_filename
	from
	(
		select [Org ID] as organisation_id, [Rank], pm_filename
			, row_number() over (partition by [Org ID],[Rank] order by case when src = 'l' then 1 else 2 end) as rn --CF028: was a FULL OUTER JOIN between f_pom_sql_narrow/l_pom_sql_narrow - replaced with UNION ALL + row_number to pick "latest if present, else first", since the join's build/probe still forced a worktable past the 8060-byte limit even with narrow projections feeding it
		from
		(
			select [Org ID], [Rank], pm_filename, 'l' as src from l_pom_sql_narrow where pm_filename is not null
			union all
			select [Org ID], [Rank], pm_filename, 'f' as src from f_pom_sql_narrow where pm_filename is not null
		) combined
	) ranked
	where rn = 1
),
POM_With_Year as --pulls the "reporting year" calc (POM's submission year + 1, the year obligation determination is keyed to) into a single named column, computed once and reused below, rather than repeating the same CASE expression in multiple places
(
	select pm.FileName
		, pm.organisation_id
		, pm.subsidiary_id
		, pm.organisation_size
		, pm.Packaging_type
		, pm.packaging_material
		, pm.packaging_material_weight
		, pm.packaging_material_units
		, pm.packaging_material_subtype
		, pm.ram_rag_rating
		, pm.transitional_packaging_units
		, pm.from_country
		, pm.to_country
		, cfm.ComplianceSchemeId
		, (case when cfm.SubmissionPeriod in ('Jan to Jun 2023','January to June 2023','July to December 2023') then 2023
				when cfm.SubmissionPeriod in ('Jan to Jun 2024','January to June 2024','July to December 2024') then 2024
				when cfm.SubmissionPeriod in ('Jan to Jun 2025','January to June 2025','July to December 2025') then 2025
				when cfm.SubmissionPeriod in ('Jan to Jun 2026','January to June 2026','July to December 2026') then 2026
				when cfm.SubmissionPeriod in ('Jan to Jun 2027','January to June 2027','July to December 2027') then 2027
				when cfm.SubmissionPeriod in ('Jan to Jun 2028','January to June 2028','July to December 2028') then 2028
				else 0
				end) + 1 as obligation_year
	from rpd.pom pm --CF026: select only the columns POM_Filtered's consumers (agg_POM family + the cascade) actually need, rather than pm.* - carrying every wide column of rpd.pom through POM_With_Year/POM_With_Obligation/the cascade's GROUP BY was forcing a worktable past SQL Server's 8060-byte row-size limit (Msg 8618)
	inner join Qualifying_POM_FileNames qfn on qfn.pm_filename = pm.FileName --CF019: prune to only the files the output can ever reference, before the expensive obligation-determination join
	inner join rpd.cosmos_file_metadata cfm on cfm.FileName = pm.FileName
),
POM_With_Obligation as --joined POM+obligation rows, before either filter below is applied
(
	select pwy.*
		, ob.obligation_status
	from POM_With_Year pwy
	left join rpd.Organisations o on o.ReferenceNumber = pwy.organisation_id
	left join dbo.t_producer_obligation_determination ob
		on ob.organisation_id = pwy.organisation_id
		and ISNULL(ob.subsidiary_id,'') = ISNULL(NULLIF(TRIM(pwy.subsidiary_id),''),'') --SubsidiaryId match (HandleMissingRegistrationData's o.SubsidiaryId == p.SubsidiaryId)
		and ob.submitter_id = COALESCE(pwy.ComplianceSchemeId, o.ExternalId) --SubmitterId match (HandleMissingRegistrationData's o.SubmitterId == p.SubmitterId)
		and ob.submission_period_year = pwy.obligation_year
),
Org_Missing_Registration_Cascade as --mirrors EPR.Calculator.Service.Function's ErrorReportService.HandleMissingRegistrationData: that check runs within a SINGLE calculator run, i.e. one reporting year at a time - "for each subsidiary's (SubsidiaryId, SubmitterId) in that year's POM data, does a matching registration/obligation row exist? if ANY subsidiary fails, flag the WHOLE organisation_id group for that year". A GROUP BY + HAVING here computes this small (org, year) exclusion set once via aggregation, then POM_Filtered below anti-joins against it - cheaper than a row-level window function spanning the full multi-year join, which forces a sort/hash over every POM line item just to test this condition. Restricted to TargetObligationYear (CF023) - other years fall through POM_Filtered unaffected by this cascade, exactly as before this change was introduced. Also restricted to Winning_POM_FileNames (CF025) - evaluating against the broader Qualifying_POM_FileNames set let a superseded "first" submission's now-dropped subsidiaries trigger a false cascade even when the file that actually feeds the output never included them
--CF033: HandleMissingRegistrationData's real input (calculator_run_pom_data_detail) is sourced from sp_GetPaycalPomData, which only ever selects Packaging_type IN ('HH','CW','PB') or (Packaging_type='HDC' and packaging_material='GL') - see sp_GetPaycalPomData-before.sql. A subsidiary whose ONLY rpd.pom rows fall outside that scope (e.g. Packaging_type='OW') never reaches the real cascade check at all, regardless of whether it has a t_producer_obligation_determination row. Without this scoping, a newly-onboarded subsidiary with only zero-weight 'OW' rows and no determination row yet was wrongly zeroing out its entire org's legitimate weight (confirmed against org 111484, where UAT includes the org's real ~979.5t untouched). Verified safe against the 3 known genuine cascade orgs (101979/100964/115361), each of which has at least one in-scope missing-determination subsidiary, so they still correctly trigger after this change
(
	select pwo.organisation_id, pwo.obligation_year
	from POM_With_Obligation pwo
	inner join Winning_POM_FileNames wfn on wfn.organisation_id = pwo.organisation_id and wfn.pm_filename = pwo.FileName
	cross join TargetObligationYear toy
	where pwo.organisation_size = 'L'
	  and pwo.obligation_year = toy.ObligationYear
	group by pwo.organisation_id, pwo.obligation_year
	having max(case when pwo.obligation_status is null
	                 and (pwo.Packaging_type in ('HH','CW','PB') or (pwo.Packaging_type = 'HDC' and pwo.packaging_material = 'GL')) --CF033
	                then 1 else 0 end) = 1
),
POM_Filtered as --CF017/CF021: rpd.pom rows restricted to subsidiaries with an Obligated ('O') determination for the following reporting year, for Large producers only - dbo.t_producer_obligation_determination (and v_producer_obligation_determination, which feeds it) only ever computes obligation status for organisation_size='L' (see v_producer_obligation_determination.sql), so Small/Medium producers are never subject to this check at all, matching Paycal's Large-producer-specific scope
(
	select pwo.*
	from POM_With_Obligation pwo
	left join Org_Missing_Registration_Cascade cascade_excl
		on cascade_excl.organisation_id = pwo.organisation_id
	   and cascade_excl.obligation_year = pwo.obligation_year
	where (pwo.organisation_size <> 'L' --CF021: not Large - obligation determination doesn't apply to this row at all, keep it unfiltered
	   or pwo.obligation_status = 'O') --CF021: Large - only keep subsidiaries explicitly marked Obligated (matches old INNER JOIN behaviour for Large; no-match still excludes, per known table-coverage-gap risk accepted for now)
	  and cascade_excl.organisation_id is null --mirrors HandleMissingRegistrationData's all-or-nothing group cascade (see Org_Missing_Registration_Cascade above)
)
select * into #PomFiltered from POM_Filtered;
