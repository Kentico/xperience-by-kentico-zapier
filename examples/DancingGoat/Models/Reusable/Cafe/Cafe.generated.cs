//--------------------------------------------------------------------------------------------------
// <auto-generated>
//
//     This code was generated by code generator tool.
//
//     To customize the code use your own partial class. For more info about how to use and customize
//     the generated code see the documentation at https://docs.xperience.io/.
//
// </auto-generated>
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using CMS.ContentEngine;

namespace DancingGoat.Models
{
	/// <summary>
	/// Represents a content item of type <see cref="Cafe"/>.
	/// </summary>
	[RegisterContentTypeMapping(CONTENT_TYPE_NAME)]
	public partial class Cafe : IContentItemFieldsSource
	{
		/// <summary>
		/// Code name of the content type.
		/// </summary>
		public const string CONTENT_TYPE_NAME = "DancingGoat.Cafe";


		/// <summary>
		/// Represents system properties for a content item.
		/// </summary>
		[SystemField]
		public ContentItemFields SystemFields { get; set; }


		/// <summary>
		/// CafeName.
		/// </summary>
		public string CafeName { get; set; }


		/// <summary>
		/// CafeIsCompanyCafe.
		/// </summary>
		public bool CafeIsCompanyCafe { get; set; }


		/// <summary>
		/// CafeStreet.
		/// </summary>
		public string CafeStreet { get; set; }


		/// <summary>
		/// CafeCity.
		/// </summary>
		public string CafeCity { get; set; }


		/// <summary>
		/// CafeCountry.
		/// </summary>
		public string CafeCountry { get; set; }


		/// <summary>
		/// CafeZipCode.
		/// </summary>
		public string CafeZipCode { get; set; }


		/// <summary>
		/// CafePhone.
		/// </summary>
		public string CafePhone { get; set; }


		/// <summary>
		/// CafePhoto.
		/// </summary>
		public IEnumerable<Image> CafePhoto { get; set; }


		/// <summary>
		/// CafeAdditionalNotes.
		/// </summary>
		public string CafeAdditionalNotes { get; set; }


		/// <summary>
		/// CafeCuppingOffer.
		/// </summary>
		public IEnumerable<Coffee> CafeCuppingOffer { get; set; }
	}
}