using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFABusinessTypes
{
    public enum bestAssetActivityTypeEnum
    {
		acInvalidActivityCode,
		acActive=1,										// 'A'
		acInactive,										// 'I'
		acDisposed,										// 'D'
		acWholeTransferDisposed,						// 'F'
		acPartiallyDisposed,							// 'J'
		acWholeTransfer,								// 'K'
		acPartialTransferWithinCmp,						// 'L'
		acPartialTransferOutsideCmp,					// 'M'
		acPartialTransferDisposed,						// 'N'
		acADIImport										// 'X'
	} ;

    public 	enum bestAssetImportCodeEnum
	{
		icNoCode,
		icOriginal,
		icPCFasImport,
		icDosFasImport,
		icPeachFasImport,
		icInvalidCode
	} ;

	public enum bestAssetITCTypeEnum {
		itcNoITC,
		itcNewPropFullCredit,
		itcNewPropReducedCredit,
		itcUsedPropFullCredit,
		itcUsedPropReducedCredit,
		itcRehab30Year,
		itcRehab40Year,
		itcCertHistoricRehab,
		itcNonCertHistoricRehab,
		itcBiomass,
		itcIntercityBuses,
		itcHydroelectricGenerating,
		itcOceanThermal,
		itcSolarEnergy,
		itcWind,
		itcGeoThermal,
		itcCertHistoricTransition,
		itcQualifiedProgressExp,
		itcReforestation,
		itcSolarEnergyProperty,
		itcOtherEnergyProperty,
		itcFuelCellProperty,
		itcMicroturbineProperty,
		itcAdvancedCoalProject,
		itcGasificationProject,
		itcUnknownITCType								// internal use only
	} ;

    public enum bestAssetDeprAdjustmentConventionEnum
    {
        dacInvalidAdjustment,
        dacNoAdjustment,
        dacImmediate,
        dacPostrecovery
    } ;

	public enum bpblBookTypeEnum
	{
        bpblBookTaxBook,
        bpblBookInternalBook,
        bpblBookStateBook,
        bpblBookAMTBook,
        bpblBookEandPBook,
        bpblBookACEBook,
        bpblBookUserBook
	} ;

    public 	enum bestAssetCreationCodeEnum
	{
		ccUnknownCreationCode,
		ccOriginalAsset,								// "O"
		ccDisposedPartOfPartialDisposal,				// "D"
		ccRemainingPartOfPartialDisposal,				// "E"
		ccTransferredPartOfPartialTransfer,				// "P"
		ccTransferredPartOfPartialTransferConsDisposed,	// "Q"
		ccRemainingPartOfPartialTransfer,				// "R"
		ccRemainingPartOfPartialTransferConsDisposed,	// "S"
		ccWholeTransfer,								// "T"
		ccDisposedPartOfPartialTransfer,				// "U"
		ccDisposedPartOfPartialTransferAnotherCompany,	// "V"
		ccWholeTransferConsideredDisposed,				// "W"
	} ;

}
