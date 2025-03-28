using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnionAvatars.API;
using UnionAvatars.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace UnionAvatars.UI
{
    public class AvatarCustomizationUI : UIModule
    {
        #region Serialized Variables

        [Header("General Component References")]
        [SerializeField]
        private GameObject loadingUI;

        [SerializeField]
        private CanvasGroup itemPanelCanvasGroup;

        [SerializeField]
        private AvatarViewUI avatarView;

        [Header("Inventory Component References")]
        [SerializeField]
        private Transform itemGrid;

        [SerializeField]
        private Scrollbar itemScrollbar;

        [SerializeField]
        private AssetSlotUI assetSlotPrefab;

        [SerializeField]
        private GameObject itemNotAvailableText;

        [SerializeField]
        private Transform brandLayout;

        [SerializeField]
        private ItemSlotUI brandSlotPrefab;

        [SerializeField]
        private GameObject outfitSubCategories;

        [SerializeField]
        private GameObject garmentSubCategories;

        [SerializeField]
        private GameObject hairSubCategories;

        [SerializeField]
        private GameObject hairColorButtons;

        [SerializeField]
        private GameObject wardrobeSubCategories;

        [SerializeField]
        private EventToggleGroup brandToggleGroup;

        [Header("Load Avatar")]
        [SerializeField]
        private InputDialog nameDialog;

        [SerializeField]
        private Button loadAvatarButton;

        #endregion

        #region Brands

        private Brand[] cachedBrands;
        private Brand _selectedBrand = null;
        private Brand selectedBrand
        {
            get => _selectedBrand;
            set
            {
                _selectedBrand = value;
                FetchAssets(true);
            }
        }

        const string DefaultBrand = "32793e4a-5743-484a-baa5-6247c6138d05";

        #endregion

        #region Assets

        // Store a reference to the last asset type selected
        // So when we press "Create Avatar" we know what to send
        private Category lastSelectedAsset = Category.Garments;
        private Outfit _selectedOutfit;
        private Outfit selectedOutfit
        {
            get => _selectedOutfit;
            set
            {
                _selectedOutfit = value;
                lastSelectedAsset = Category.Outfits;
            }
        }
        private Garment[] _selectedGarments;
        private Garment[] selectedGarments
        {
            get => _selectedGarments;
            set
            {
                _selectedGarments = value;
                lastSelectedAsset = Category.Garments;
            }
        }
        private Hair selectedHair;

        #endregion

        # region Categories and Sub-Categories

        public enum Category
        {
            Garments,
            Outfits,
            Hairs,
            Wardrobe
        }

        private Category _selectedCategory = Category.Garments;
        private Category selectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                selectedSubCategory = 0;
                ShowSubCategories();
            }
        }

        private int _selectedSubCategory = 0;
        private int selectedSubCategory
        {
            get => _selectedSubCategory;
            set
            {
                _selectedSubCategory = value;
                FetchAssets(true);
            }
        }

        private int itemCurrentPage = 1;
        private int totalCurrentItems = 0;

        #endregion

        #region Hair Colors

        private Color[] hairColors = new Color[]
        {
            Color.black,
            new Color(0.402f, 0.248f, 0.129f),
            new Color(0.800f, 0.640f, 0.374f),
            new Color(0.656f, 0.656f, 0.656f),
            new Color(0.756f, 0.314f, 0.103f),
            new Color(0.238f, 0.671f, 0.800f),
            new Color(0.800f, 0.279f, 0.731f)
        };
        private Color _selectedHairColor = Color.black;
        private Color selectedHairColor
        {
            get => _selectedHairColor;
            set
            {
                _selectedHairColor = value;
                UpdateHairColor();
            }
        }

        #endregion

        # region State Variables

        private int _activeLoadingTaks = 0;
        private bool isLoading
        {
            get => _activeLoadingTaks > 0;
            set
            {
                if (value)
                    _activeLoadingTaks++;
                else
                    _activeLoadingTaks--;

                ToggleLoadingUI();
            }
        }

        private AvatarMetadata avatarData;
        private Catalogue catalogue;

        private int garmentVersion;
        private int outfitVersion;
        private int[] hairVersion;

        public Action<string, Outfit, Hair, Garment[], Color> OnAvatarFinished;

        #endregion

        public async void InitializeCreationModule(AvatarMetadata avatar)
        {
            if (avatar == null)
                throw new ArgumentNullException("avatar");

            // Store the avatar data reference for retrieving its information later
            avatarData = avatar;

            // TODO: Refactor. Extract into static helper
            SetAssetVersionsForStyle(avatarData.Style);

            try
            {
                isLoading = true;

                Catalogue[] catalogues = await uiManager.session.GetCatalogues();

                if (cancellationToken.IsCancellationRequested)
                    return;

                if (catalogues == null)
                {
                    (root as BaseModule).GoBack(false);
                    return;
                }

                if (catalogues.Length <= 0)
                {
                    uiManager
                        .session
                        .LogHandler
                        .APIWarning("The organization provided doesn't have any asset catalogues");
                    (root as BaseModule).GoBack(false);
                    return;
                }

                // Temporary, select the first catalogue in the API
                catalogue = catalogues[0];

                Head avatarHead = await uiManager.session.GetHead(avatar.HeadId);
                if (avatarHead == null)
                {
                    (root as BaseModule).GoBack(false);
                    return;
                }

                if (cancellationToken.IsCancellationRequested)
                    return;

                // Get the hair color
                selectedHairColor = avatarHead.Metadata?.HairColor ?? selectedHairColor;

                // Set gender, style and version in avatar view (used in garment assemble)
                avatarView.selectedGender = avatarData.Gender;
                avatarView.selectedStyle = avatarData.Style;
                avatarView.selectedVersion = garmentVersion;
                avatarView.selectedHairColor = selectedHairColor;

                await avatarView.SetHeadCache(avatarHead);

                if (cancellationToken.IsCancellationRequested)
                    return;

                await avatarView.BuildBaseBodyForGarments();

                await LoadDefaultAssets(avatarHead);

                if (cancellationToken.IsCancellationRequested)
                    return;

                // Temporary workaround
                // If gender is N/A, disable garments
                if (avatarData.Gender == Gender.all)
                {
                    transform.FindBFS("Garments").gameObject.SetActive(false);
                    _selectedCategory = Category.Outfits;
                }
                else
                {
                    _selectedCategory = Category.Garments; // Default window
                }
                ShowSubCategories();

                FetchBrands();
            }
            catch (Exception e)
            {
                uiManager.session.LogHandler.UIError(e.Message);
                (root as BaseModule).GoBack(false);
            }
            finally
            {
                isLoading = false;
            }
        }

        private void SetAssetVersionsForStyle(Style style)
        {
            // Hardcoded version
            switch (style)
            {
                case Style.phr:
                    garmentVersion = 4;
                    outfitVersion = 4;
                    hairVersion = new int[] { 3, 4 };
                    break;
                case Style.crt:
                    garmentVersion = 1;
                    outfitVersion = 1;
                    hairVersion = new int[] { 1 };
                    break;
                default:
                    garmentVersion = 4;
                    outfitVersion = 4;
                    hairVersion = new int[] { 3, 4 };
                    break;
            }
        }

        private async Task LoadDefaultAssets(Head avatarHead)
        {
            // If the avatar is new (not being edited)
            if (avatarData.Id == Guid.Empty)
            {
                // If no gender selected, then load an outfit instead of garments
                if (avatarData.Gender == Gender.all)
                {
                    Outfit defaultOutfit = await uiManager
                        .session
                        .GetAsset<Outfit>(Constants.GetDefaultAssetID(avatarData.Gender, avatarData.Style, "Outfit"));
                    if (cancellationToken.IsCancellationRequested)
                        return;
                    // Load default body in avatar view
                    selectedOutfit = defaultOutfit;
                    await avatarView.LoadAvatarView(defaultOutfit);
                }
                else
                {
                    Garment defaultTop = await uiManager
                        .session
                        .GetAsset<Garment>(Constants.GetDefaultAssetID(avatarData.Gender, avatarData.Style, "Top"));
                    Garment defaultBottom = await uiManager
                        .session
                        .GetAsset<Garment>(Constants.GetDefaultAssetID(avatarData.Gender, avatarData.Style, "Bottom"));
                    Garment defaultShoes = await uiManager
                        .session
                        .GetAsset<Garment>(Constants.GetDefaultAssetID(avatarData.Gender, avatarData.Style, "Shoes"));

                    if (cancellationToken.IsCancellationRequested)
                        return;

                    selectedGarments = new Garment[] { null, defaultTop, defaultBottom, defaultShoes };

                    await avatarView.LoadAvatarViewWithGarments(selectedGarments);
                }
            }
            else // The avatar already exists and it's being edited
            {
                // Get the garments (or outfit) that the avatar is using
                AvatarParts avatarParts = await uiManager.session.GetAvatarParts(avatarData);

                if (cancellationToken.IsCancellationRequested)
                    return;

                // Get the correct gender of the avatar
                avatarData.Gender = avatarParts.Gender;

                Garment defaultTop = await uiManager
                    .session
                    .GetAsset<Garment>(Constants.GetDefaultAssetID(avatarData.Gender, avatarData.Style, "Top"));
                Garment defaultBottom = await uiManager
                    .session
                    .GetAsset<Garment>(Constants.GetDefaultAssetID(avatarData.Gender, avatarData.Style, "Bottom"));
                Garment defaultShoes = await uiManager
                    .session
                    .GetAsset<Garment>(Constants.GetDefaultAssetID(avatarData.Gender, avatarData.Style, "Shoes"));

                if (cancellationToken.IsCancellationRequested)
                    return;

                selectedGarments = new Garment[] { null, defaultTop, defaultBottom, defaultShoes };

                if (avatarParts.Outfit != null)
                {
                    // Load default body in avatar view
                    selectedOutfit = avatarParts.Outfit;
                    await avatarView.LoadAvatarView(avatarParts.Outfit);
                }
                else
                {
                    selectedGarments = new Garment[]
                    {
                        avatarParts.Accessories,
                        avatarParts.Top,
                        avatarParts.Bottom,
                        avatarParts.Shoes
                    };

                    await avatarView.LoadAvatarViewWithGarments(selectedGarments);
                }
            }

            selectedHair =
                avatarHead.Hair
                ?? await uiManager
                    .session
                    .GetAsset<Hair>(Constants.GetDefaultAssetID(avatarData.Gender, avatarData.Style, "Hair"));

            if (cancellationToken.IsCancellationRequested)
                return;

            _ = avatarView.LoadHair(selectedHair);
        }

        private async void FetchBrands()
        {
            isLoading = true;

            Brand[] brands = await uiManager.session.GetBrands();

            if (cancellationToken.IsCancellationRequested)
                return;

            if (brands == null)
            {
                uiManager.session.LogHandler.APIWarning("Couldn't fetch brand information");
                selectedBrand = null;
                return;
            }

            // Download and set brand logos
            List<Brand> filteredBrands = new List<Brand>();
            foreach (Brand brand in brands)
            {
                ItemSlotUI brandSlot = Instantiate(brandSlotPrefab, brandLayout);
                if (await brandSlot.SetupSlot(brand.Logo, cancellationToken)) // Remove brands whose pic couldn't be downloaded
                {
                    // Ensure the first brand always displays first
                    if (brand.Id.ToString() == DefaultBrand)
                    {
                        filteredBrands.Insert(0, brand);
                        brandSlot.transform.SetAsFirstSibling();
                    }
                    else
                    {
                        filteredBrands.Add(brand);
                        brandSlot.transform.SetAsLastSibling();
                    }
                }
            }

            cachedBrands = filteredBrands.ToArray();
            brandToggleGroup.RefreshToggles(0);

            selectedBrand = cachedBrands[0]; // Setter -> Refresh the collection list

            isLoading = false;
        }

        private void ShowSubCategories()
        {
            Dictionary<Category, GameObject> subCategories = new Dictionary<Category, GameObject>()
            {
                { Category.Outfits, outfitSubCategories },
                { Category.Garments, garmentSubCategories },
                { Category.Hairs, hairSubCategories },
                { Category.Wardrobe, wardrobeSubCategories },
            };

            foreach (var subCategory in subCategories)
            {
                subCategory.Value.SetActive(subCategory.Key == selectedCategory);
            }

            // Display color buttons but only for hair
            hairColorButtons.SetActive(selectedCategory == Category.Hairs);
        }

        private async void FetchAssets(bool resetPage)
        {
            isLoading = true;

            if (resetPage)
            {
                // Delete all previous items
                foreach (Transform child in itemGrid)
                {
                    // TODO: Add a pool of objects, or reuse the existing ones
                    Destroy(child.gameObject);
                }
            }

            itemCurrentPage = resetPage ? 1 : (itemCurrentPage + 1);

            // TODO: Refactor this
            switch (selectedCategory)
            {
                case Category.Outfits:
                    await FilterAndLoadItems<Outfit>(
                        await uiManager
                            .session
                            .GetAssets<Outfit>(
                                catalogue.Id,
                                page: itemCurrentPage,
                                type: AssetType.outfits,
                                sourceType: new SourceType[] { SourceType.@default },
                                gender: avatarData.Gender,
                                style: avatarData.Style,
                                version: new int[] { outfitVersion },
                                brand: selectedBrand
                            )
                    );
                    break;
                case Category.Garments:
                    await FilterAndLoadItems<Garment>(
                        await uiManager
                            .session
                            .GetAssets<Garment>(
                                catalogue.Id,
                                page: itemCurrentPage,
                                type: AssetType.garments,
                                sourceType: new SourceType[] { SourceType.@default },
                                gender: avatarData.Gender,
                                style: avatarData.Style,
                                version: new int[] { garmentVersion },
                                brand: selectedBrand
                            )
                    );
                    break;
                case Category.Hairs:
                    await FilterAndLoadItems<Hair>(
                        await uiManager
                            .session
                            .GetAssets<Hair>(
                                catalogue.Id,
                                page: itemCurrentPage,
                                type: AssetType.hairs,
                                sourceType: new SourceType[] { SourceType.@default },
                                gender: avatarData.Gender,
                                style: avatarData.Style,
                                version: hairVersion,
                                brand: selectedBrand
                            )
                    );
                    break;
                case Category.Wardrobe:
                    switch (selectedSubCategory)
                    {
                        case 0: // Accessories
                        case 1: // Tops
                        case 2: // Bottoms
                        case 3: // Shoes
                            await FilterAndLoadItems<Garment>(
                                await uiManager
                                    .session
                                    .GetWardrobe<Garment>(type: AssetType.garments, page: itemCurrentPage)
                            );
                            break;
                        case 4: // Outfits
                            await FilterAndLoadItems<Outfit>(
                                await uiManager
                                    .session
                                    .GetWardrobe<Outfit>(type: AssetType.outfits, page: itemCurrentPage)
                            );
                            break;
                    }
                    break;
            }

            if (cancellationToken.IsCancellationRequested)
                return;

            itemNotAvailableText.SetActive(itemGrid.childCount == 0); // If no items, show message

            isLoading = false;
        }

        private async Task FilterAndLoadItems<T>(Paginated<T> itemsPaginated)
            where T : UnionAsset
        {
            if (itemsPaginated == null)
                return;

            totalCurrentItems = itemsPaginated.Total;

            Paginated<T> filteredItems = FilterItems<T>(itemsPaginated); // Get filtered items

            await LoadItemSlots<T>(filteredItems.Items);
        }

        /// <summary>
        /// This method will filter items by gender and subcategory
        /// </summary>
        /// <typeparam name="T">Type of the items (Outfit, Hair, Garment,...)</typeparam>
        /// <returns>Item array</returns>
        private Paginated<T> FilterItems<T>(Paginated<T> itemsPaginated)
            where T : UnionAsset
        {
            switch (itemsPaginated)
            {
                case Paginated<Outfit> outfits:
                    // Get rid of outfits without metadata to prevent bugs
                    outfits.Items = outfits.Items.Where(outfit => outfit?.Metadata?.Body != null).ToArray();
                    break;

                case Paginated<Garment> garments:
                    switch (selectedSubCategory) // Filter by subcategory
                    {
                        case 0: // Accessories
                            // Add an extra slot to select "None"
                            Garment noneGarment = new Garment()
                            {
                                Name = "None",
                                ThumbnailUrl = new Uri("https://app.unionavatars.com/no-item.png"),
                                Gender = Gender.male
                            };

                            if (itemsPaginated.Page == 1)
                                garments.Items = garments
                                    .Items
                                    .Where(garment => garment?.Type == "accessories")
                                    .Prepend(noneGarment)
                                    .ToArray();
                            else
                                garments.Items = garments
                                    .Items
                                    .Where(garment => garment?.Type == "accessories")
                                    .ToArray();
                            break;
                        case 1: // Tops
                            garments.Items = garments.Items.Where(garment => garment?.Type == "top").ToArray();
                            break;
                        case 2: // Bottoms
                            garments.Items = garments.Items.Where(garment => garment?.Type == "bottom").ToArray();
                            break;
                        case 3: // Shoes
                            garments.Items = garments.Items.Where(garment => garment?.Type == "shoes").ToArray();
                            break;
                    }
                    break;

                case Paginated<Hair> hairs:
                    return itemsPaginated; // No filter needed for hairs

                default:
                {
                    itemsPaginated.Items = Array.Empty<T>();
                    return itemsPaginated;
                }
            }

            return itemsPaginated;
        }

        /// <summary>
        /// Creates slot instances and loads them into the UI
        /// </summary>
        /// <param name="itemList">The item of outfits, hairs or garments</param>
        /// <param name="onClick">Function that will get called when the item is selected</param>
        /// <typeparam name="T">The type of the item selected</typeparam>
        private async Task LoadItemSlots<T>(T[] itemList)
            where T : UnionAsset
        {
            List<Task> slotSetupTasks = new List<Task>();

            // We check for the paid assets here in order to display correctly the slot
            // Otherwise it will still show the price
            PaidAssets paidAssets = await uiManager.session.GetPaidAssets();

            for (int i = 0; i < itemList.Length; i++)
            {
                T item = itemList[i];

                AssetSlotUI assetSlot = Instantiate(assetSlotPrefab, itemGrid);

                // Create a task for downloading the thumbnail
                slotSetupTasks.Add(
                    assetSlot.SetupSlot(item, cancellationToken, paidAssets.Assets.Contains(item.ContainerId))
                );

                // Setup an event action to load an avatar when the slot gets pressed
                switch (item)
                {
                    case Outfit outfit:
                        assetSlot
                            .GetComponent<Button>()
                            .onClick
                            .AddListener(() =>
                            {
                                selectedOutfit = outfit;
                                _ = avatarView.LoadAvatarView(outfit);
                            });
                        break;
                    case Garment garment:
                        assetSlot
                            .GetComponent<Button>()
                            .onClick
                            .AddListener(() =>
                            {
                                SelectGarment(garment);
                            });
                        break;
                    case Hair hair:
                        assetSlot
                            .GetComponent<Button>()
                            .onClick
                            .AddListener(() =>
                            {
                                selectedHair = hair;
                                _ = avatarView.LoadHair(hair);
                            });
                        break;
                }
            }

            await Task.WhenAll(slotSetupTasks);
        }

        private void SelectGarment(Garment selectedGarment)
        {
            // Check if the garment URL is null (In case of the "None" option, this is true)
            // If so, replace the garment slot for a null
            selectedGarments[(int)selectedSubCategory] = (selectedGarment.Url == null) ? null : selectedGarment;
            lastSelectedAsset = Category.Garments;

            _ = avatarView.LoadAvatarViewWithGarments(selectedGarments);
        }

        private void UpdateHairColor()
        {
            if (selectedHair == null)
                return;

            // Handle the edge case of the cap, where we don't want any color applied
            if (!selectedHair.Name.Contains("_cap"))
            {
                avatarView.ChangeHairColor(selectedHairColor);
            }
            else
            {
                avatarView.selectedHairColor = selectedHairColor;
            }
        }

        private void ToggleLoadingUI()
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            itemPanelCanvasGroup.interactable = !isLoading;
            loadAvatarButton.interactable = !isLoading;
            loadingUI.SetActive(isLoading);
        }

        public void OnUpdateScrollbar(float value)
        {
            // If the scrollbar gets close to the end, load more items
            if (value < 0.1f && !isLoading && (itemCurrentPage * 20 < totalCurrentItems))
                FetchAssets(false);
        }

        private void Update()
        {
            bool allItemsShowing = itemCurrentPage * 20 < totalCurrentItems;

            // Fetch items if there are slots available and items in the catalogue
            if (itemScrollbar.value > 0.1f && !isLoading && allItemsShowing && itemGrid.childCount <= 10)
                FetchAssets(false);

            // Fetch items if users scrolls down and there are items in the catalogue
            if (itemScrollbar.value <= 0.1f && !isLoading && allItemsShowing)
                FetchAssets(false);
        }

        private async void FinishAvatar(string name)
        {
            if (lastSelectedAsset == Category.Outfits)
            {
                OnAvatarFinished.Invoke(name, selectedOutfit, selectedHair, null, selectedHairColor);
            }
            else if (lastSelectedAsset == Category.Garments)
            {
                Outfit outfit = null;

                if (avatarData.OutfitId != Guid.Empty)
                    outfit = await uiManager.session.GetOutfit(avatarData.OutfitId);

                if (cancellationToken.IsCancellationRequested)
                    return;

                OnAvatarFinished.Invoke(name, outfit, selectedHair, selectedGarments, selectedHairColor);
            }
            else
            {
                throw new InvalidOperationException("Unrecognized view type during avatar creation");
            }
        }

        #region Public Button Methods

        // Shows an input window to input the avatar name once the creation ends
        public void ShowNamePrompt()
        {
            loadAvatarButton.interactable = false;

            InputDialog newDialog = Instantiate(nameDialog, transform);
            newDialog.SetupDialog(
                "Give your avatar a name:",
                () =>
                {
                    newDialog.Close();
                    loadAvatarButton.interactable = true;
                },
                (name) =>
                {
                    if (name.Length > 0 && Regex.IsMatch(name, @"^[a-zA-Z0-9_ ]+$"))
                    {
                        newDialog.Close();
                        FinishAvatar(name);
                        loadAvatarButton.interactable = true;
                    }
                    else
                    {
                        uiManager
                            .session
                            .LogHandler
                            .CustomLog("Invalid Name", "Use only characters, numbers and underscores");
                    }
                },
                avatarData.Name
            );
        }

        public void SelectBrand(int brand)
        {
            selectedBrand = cachedBrands[brand];
        }

        public void SelectCategory(int category)
        {
            // Same order as enum
            selectedCategory = (Category)category;
        }

        public void SelectSubCategory(int subCategory)
        {
            selectedSubCategory = subCategory;
        }

        public void SelectHairColor(int hairColor)
        {
            selectedHairColor = hairColors[hairColor];
        }

        #endregion
    }
}
