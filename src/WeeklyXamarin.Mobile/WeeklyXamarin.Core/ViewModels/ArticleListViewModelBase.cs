﻿using MvvmHelpers;
using MvvmHelpers.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WeeklyXamarin.Core.Helpers;
using WeeklyXamarin.Core.Models;
using WeeklyXamarin.Core.Services;
using Xamarin.Essentials;
using Xamarin.Essentials.Interfaces;

namespace WeeklyXamarin.Core.ViewModels
{
    public abstract class ArticleListViewModelBase : ArticleViewModelBase
    {
        protected readonly IBrowser browser;
        protected readonly IPreferences preferences;
        private ListState currentState;
        private ObservableRangeCollection<Article> articles = new ObservableRangeCollection<Article>();

        public ArticleListViewModelBase(INavigationService navigation,
            IAnalytics analytics,
            IDataStore dataStore,
            IBrowser browser,
            IPreferences preferences,
            IShare share,
            IMessagingService messagingService) : base(navigation, share, dataStore, analytics, messagingService)
        {
            OpenArticleCommand = new AsyncCommand<Article>(OpenArticle);
            SearchCategoryCommand = new Command<Article>(ExecuteCategorySearch);

            this.browser = browser;
            this.preferences = preferences;
            messagingService.Subscribe<Article>(this, "BOOKMARKED", BookMarkChanged);

        }

        private void BookMarkChanged(Article article)
        {
            // does our article collection have this article id
            var existingArcticle = Articles.FirstOrDefault(a => a.Id == article.Id);
            if (existingArcticle != null)
                existingArcticle.IsSaved = article.IsSaved;
        }

        public ICommand LoadArticlesCommand { get; set; }
        public ICommand OpenArticleCommand { get; set; }
        public ICommand SearchCategoryCommand { get; set; }

        public ObservableRangeCollection<Article> Articles {
            get => articles;
            set => SetProperty(ref articles, value);
        }
        public ListState CurrentState
        {
            get => currentState;
            set => SetProperty(ref currentState, value);
        }

        private async Task OpenArticle(Article article)
        {
            await browser.OpenAsync(article.Url, new BrowserLaunchOptions
            {
                LaunchMode = preferences.Get(Constants.Preferences.OpenLinksInApp, true) ? BrowserLaunchMode.SystemPreferred : BrowserLaunchMode.External,
                TitleMode = BrowserTitleMode.Show
            });
        }

        private void ExecuteCategorySearch(Article article)
        {
            navigation.GoToAsync(Constants.Navigation.Paths.Search, Constants.Navigation.ParameterNames.Category, WebUtility.UrlEncode(article.Category));
        }
    }
}
