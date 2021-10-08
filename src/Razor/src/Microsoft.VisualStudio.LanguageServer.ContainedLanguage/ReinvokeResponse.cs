﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See License.txt in the project root for license information.

using Microsoft.VisualStudio.LanguageServer.Client;

#nullable enable

namespace Microsoft.VisualStudio.LanguageServer.ContainedLanguage
{
    internal struct ReinvokeResponse<TOut>
    {
        public ILanguageClient LanguageClient { get; }

        public TOut Result { get; }

        public bool IsSuccess => LanguageClient != default;

        public ReinvokeResponse(
            ILanguageClient languageClient,
            TOut result)
        {
            LanguageClient = languageClient;
            Result = result;
        }
    }
    internal struct ReinvokeResponse2<TOut>
    {
        public string LanguageClientName { get; }

        public TOut Result { get; }

        public bool IsSuccess => LanguageClientName != default;

        public ReinvokeResponse2(
            string languageClientName,
            TOut result)
        {
            LanguageClientName = languageClientName;
            Result = result;
        }
    }
}
