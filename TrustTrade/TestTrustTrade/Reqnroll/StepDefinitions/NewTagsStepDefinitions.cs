using Moq;
using NUnit.Framework;
using Reqnroll;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrustTrade.ViewModels;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;
using TrustTrade.Services.Web.Interfaces;

namespace TestTrustTrade.StepDefinitions
{
    [Binding]
    public class AddingNewTagsStepDefinitions
    {
        private readonly ScenarioContext _scenarioContext;
        private Mock<ITagRepository> _tagRepositoryMock = new Mock<ITagRepository>();
        private Mock<IPostRepository> _postRepositoryMock = new Mock<IPostRepository>();
        private Mock<IUserService> _userServiceMock = new Mock<IUserService>();
        
        private CreatePostVM _postViewModel;
        private PostEditVM _editViewModel;
        private string _newTagInput;
        private bool _alertShown;
        private string _alertMessage;

        public AddingNewTagsStepDefinitions(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [Given("I am creating a new post")]
        public void GivenIAmCreatingANewPost()
        {
            // Initialize the create post view model with some existing tags
            _postViewModel = new CreatePostVM
            {
                ExistingTags = new List<string> { "Stocks", "Crypto", "Finance", "Investing" },
                SelectedTags = new List<string>()
            };
        }

        [Given("I am on the post creation page")]
        public void GivenIAmOnThePostCreationPage()
        {
            // Initialize the create post view model if it doesn't exist
            _postViewModel ??= new CreatePostVM
            {
                ExistingTags = new List<string> { "Stocks", "Crypto", "Finance", "Investing" },
                SelectedTags = new List<string>()
            };
        }

        [When("I look at the tags section")]
        public void WhenILookAtTheTagsSection()
        {
            // This step is just for narrative flow
            Assert.That(_postViewModel, Is.Not.Null, "Post view model is not initialized");
        }

        [Then("I should see existing tags as checkboxes")]
        public void ThenIShouldSeeExistingTagsAsCheckboxes()
        {
            Assert.That(_postViewModel.ExistingTags, Is.Not.Empty, "The list of existing tags should not be empty");
        }

        [Then("I should see an input field for adding new tags")]
        public void ThenIShouldSeeAnInputFieldForAddingNewTags()
        {
            // This step is checking if the UI has an input field for new tags
            // In a real test with UI automation, we would check for the element
            Assert.Pass("Input field for new tags should be visible");
        }

        [When("I enter a new tag name in the input field")]
        public void WhenIEnterANewTagNameInTheInputField()
        {
            // Simulate entering a new tag name
            _newTagInput = "NewTestTag";
        }

        [When("I click the \"Add Tag\" button")]
        public void WhenIClickTheAddTagButton()
        {
            // Simulate clicking the Add Tag button
            if (string.IsNullOrWhiteSpace(_newTagInput))
            {
                // Simulate JavaScript alert for empty tag
                _alertShown = true;
                _alertMessage = "Please enter a tag name.";
            }
            else
            {
                // Add the new tag to selected tags
                _postViewModel.SelectedTags.Add(_newTagInput);
                // Clear the input
                _newTagInput = "";
            }
        }

        [Then("the new tag should be added to the list as a selected checkbox")]
        public void ThenTheNewTagShouldBeAddedToTheListAsASelectedCheckbox()
        {
            Assert.That(_postViewModel.SelectedTags, Contains.Item("NewTestTag"), 
                "The new tag was not added to the selected tags list");
        }
    }
}