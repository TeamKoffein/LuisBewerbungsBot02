﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Bewerbungs.Bot.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Strings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Strings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Microsoft.Bot.Builder.Location.Resources.Strings", typeof(Strings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to , .
        /// </summary>
        internal static string AddressSeparator {
            get {
                return ResourceManager.GetString("AddressSeparator", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Add this address to your favorite locations? Reply “yes” or “no.”.
        /// </summary>
        internal static string AddToFavoritesAsk {
            get {
                return ResourceManager.GetString("AddToFavoritesAsk", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sorry, please reply “yes” or “no” to add the address to your favorite locations..
        /// </summary>
        internal static string AddToFavoritesRetry {
            get {
                return ResourceManager.GetString("AddToFavoritesRetry", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please provide the {0}..
        /// </summary>
        internal static string AskForEmptyAddressTemplate {
            get {
                return ResourceManager.GetString("AskForEmptyAddressTemplate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to OK {0}..
        /// </summary>
        internal static string AskForPrefix {
            get {
                return ResourceManager.GetString("AskForPrefix", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to  Please also provide the {0}..
        /// </summary>
        internal static string AskForTemplate {
            get {
                return ResourceManager.GetString("AskForTemplate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to cancel.
        /// </summary>
        internal static string CancelCommand {
            get {
                return ResourceManager.GetString("CancelCommand", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to OK, cancelled..
        /// </summary>
        internal static string CancelPrompt {
            get {
                return ResourceManager.GetString("CancelPrompt", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to OK, I will ship to {0}. Is that correct? Enter &apos;yes&apos; or &apos;no&apos;..
        /// </summary>
        internal static string ConfirmationAsk {
            get {
                return ResourceManager.GetString("ConfirmationAsk", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to I didn&apos;t understand. Enter &apos;yes&apos; or &apos;no&apos;..
        /// </summary>
        internal static string ConfirmationInvalidResponse {
            get {
                return ResourceManager.GetString("ConfirmationInvalidResponse", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to country.
        /// </summary>
        internal static string Country {
            get {
                return ResourceManager.GetString("Country", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to delete.
        /// </summary>
        internal static string DeleteCommand {
            get {
                return ResourceManager.GetString("DeleteCommand", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Ok, {0} will remain a favorite location..
        /// </summary>
        internal static string DeleteFavoriteAbortion {
            get {
                return ResourceManager.GetString("DeleteFavoriteAbortion", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Are you sure you want to delete {0} from your favorite locations?.
        /// </summary>
        internal static string DeleteFavoriteConfirmationAsk {
            get {
                return ResourceManager.GetString("DeleteFavoriteConfirmationAsk", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to How would you like to choose a location?.
        /// </summary>
        internal static string DialogStartBranchAsk {
            get {
                return ResourceManager.GetString("DialogStartBranchAsk", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} is already listed in your favorites. Please type or say a different name for this address..
        /// </summary>
        internal static string DuplicateFavoriteNameResponse {
            get {
                return ResourceManager.GetString("DuplicateFavoriteNameResponse", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to edit.
        /// </summary>
        internal static string EditCommand {
            get {
                return ResourceManager.GetString("EditCommand", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Ok, let’s edit {0}. Type or say a new address..
        /// </summary>
        internal static string EditFavoritePrompt {
            get {
                return ResourceManager.GetString("EditFavoritePrompt", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please type or say a friendly name for this address, for example “Home” or “Work.”.
        /// </summary>
        internal static string EnterNewFavoriteLocationName {
            get {
                return ResourceManager.GetString("EnterNewFavoriteLocationName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to OK, I added {0} to your favorite locations..
        /// </summary>
        internal static string FavoriteAddedConfirmation {
            get {
                return ResourceManager.GetString("FavoriteAddedConfirmation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Ok, I deleted {0} from your favorite locations..
        /// </summary>
        internal static string FavoriteDeletedConfirmation {
            get {
                return ResourceManager.GetString("FavoriteDeletedConfirmation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Ok, I updated {0} with the new address {1}..
        /// </summary>
        internal static string FavoriteEdittedConfirmation {
            get {
                return ResourceManager.GetString("FavoriteEdittedConfirmation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Favorite Locations.
        /// </summary>
        internal static string FavoriteLocations {
            get {
                return ResourceManager.GetString("FavoriteLocations", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to help.
        /// </summary>
        internal static string HelpCommand {
            get {
                return ResourceManager.GetString("HelpCommand", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Say or type a valid address when asked, and I will try to find it using Bing. You can provide the full address information (street no. / name, city, region, postal/zip code, country) or a part of it. If you want to change the address, say or type &apos;reset&apos;. Finally, say or type &apos;cancel&apos; to exit without providing an address..
        /// </summary>
        internal static string HelpMessage {
            get {
                return ResourceManager.GetString("HelpMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sorry, &quot;{0}” does not match a favorite location. Type or say a number/name to use a favorite location or type “other” to create a new location. Type or say “edit” or “delete” number/name to change a favorite. Say “cancel” to exit..
        /// </summary>
        internal static string InvalidFavoriteLocationSelection {
            get {
                return ResourceManager.GetString("InvalidFavoriteLocationSelection", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please enter a valid name for this address..
        /// </summary>
        internal static string InvalidFavoriteNameResponse {
            get {
                return ResourceManager.GetString("InvalidFavoriteNameResponse", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Type or say a number to choose the address, or enter &apos;cancel&apos; to exit..
        /// </summary>
        internal static string InvalidLocationResponse {
            get {
                return ResourceManager.GetString("InvalidLocationResponse", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Tap on Send Location to proceed; type or say cancel to exit..
        /// </summary>
        internal static string InvalidLocationResponseFacebook {
            get {
                return ResourceManager.GetString("InvalidLocationResponseFacebook", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Tap one of the options to proceed; type or say cancel to exit..
        /// </summary>
        internal static string InvalidStartBranchResponse {
            get {
                return ResourceManager.GetString("InvalidStartBranchResponse", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to city or locality.
        /// </summary>
        internal static string Locality {
            get {
                return ResourceManager.GetString("Locality", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to I could not find this address. Please try again..
        /// </summary>
        internal static string LocationNotFound {
            get {
                return ResourceManager.GetString("LocationNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to I found these results. Type or say a number to choose the address, or enter &apos;other&apos; to select another address..
        /// </summary>
        internal static string MultipleResultsFound {
            get {
                return ResourceManager.GetString("MultipleResultsFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You do not seem to have any favorite locations at the moment. Enter an address and you will be able to save it to your favorite locations..
        /// </summary>
        internal static string NoFavoriteLocationsFound {
            get {
                return ResourceManager.GetString("NoFavoriteLocationsFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to other.
        /// </summary>
        internal static string OtherComand {
            get {
                return ResourceManager.GetString("OtherComand", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Other Location.
        /// </summary>
        internal static string OtherLocation {
            get {
                return ResourceManager.GetString("OtherLocation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to zip or postal code.
        /// </summary>
        internal static string PostalCode {
            get {
                return ResourceManager.GetString("PostalCode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to state or region.
        /// </summary>
        internal static string Region {
            get {
                return ResourceManager.GetString("Region", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to reset.
        /// </summary>
        internal static string ResetCommand {
            get {
                return ResourceManager.GetString("ResetCommand", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to OK, let&apos;s start over..
        /// </summary>
        internal static string ResetPrompt {
            get {
                return ResourceManager.GetString("ResetPrompt", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Type or say a number/name to use one of your favorite locations. Type or say “other” to specify a different location. To edit or delete a favorite, type or say, “edit” or “delete” number/name..
        /// </summary>
        internal static string SelectFavoriteLocationPrompt {
            get {
                return ResourceManager.GetString("SelectFavoriteLocationPrompt", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Select a location.
        /// </summary>
        internal static string SelectLocation {
            get {
                return ResourceManager.GetString("SelectLocation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to I found this result. Is this the correct address?.
        /// </summary>
        internal static string SingleResultFound {
            get {
                return ResourceManager.GetString("SingleResultFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to street address.
        /// </summary>
        internal static string StreetAddress {
            get {
                return ResourceManager.GetString("StreetAddress", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to  Type or say an address..
        /// </summary>
        internal static string TitleSuffix {
            get {
                return ResourceManager.GetString("TitleSuffix", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to  Tap &apos;Send Location&apos; to choose an address..
        /// </summary>
        internal static string TitleSuffixFacebook {
            get {
                return ResourceManager.GetString("TitleSuffixFacebook", resourceCulture);
            }
        }
    }
}
