/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

import * as vscode from 'vscode';
import { RequestType } from 'vscode-languageclient';
import { RazorDocumentManager } from '../RazorDocumentManager';
import { RazorLanguageServerClient } from '../RazorLanguageServerClient';
import { RazorLogger } from '../RazorLogger';
import { convertRangeFromSerializable } from '../RPC/SerializableRange';
import { SerializableSemanticTokensParams } from '../RPC/SerializableSemanticTokensParams';
import { ProvideSemanticTokensResponse } from './ProvideSemanticTokensResponse';
import { SemanticTokensResponse } from './SemanticTokensResponse';

export class SemanticTokensHandler {
    private static readonly getSemanticTokensEndpoint = 'razor/provideSemanticTokensRange';
    private semanticTokensRequestType: RequestType<SerializableSemanticTokensParams, ProvideSemanticTokensResponse, any, any> = new RequestType(SemanticTokensHandler.getSemanticTokensEndpoint);
    private emptySemanticTokensResponse: ProvideSemanticTokensResponse = new ProvideSemanticTokensResponse(
        new SemanticTokensResponse(new Array<number>(), true, ''),
        true,
        null);

    constructor(
        private readonly documentManager: RazorDocumentManager,
        private readonly serverClient: RazorLanguageServerClient,
        private readonly logger: RazorLogger) {
    }

    public register() {
        // tslint:disable-next-line: no-floating-promises
        this.serverClient.onRequestWithParams<SerializableSemanticTokensParams, ProvideSemanticTokensResponse, any, any>(
            this.semanticTokensRequestType,
            async (request, token) => this.getSemanticTokens(request, token));
    }

    private async getSemanticTokens(
        semanticTokensParams: SerializableSemanticTokensParams,
        cancellationToken: vscode.CancellationToken): Promise<ProvideSemanticTokensResponse> {
        try {
            const razorDocumentUri = vscode.Uri.parse(semanticTokensParams.textDocument.uri);
            const razorDocument = await this.documentManager.getDocument(razorDocumentUri);
            if (razorDocument === undefined) {
                return this.emptySemanticTokensResponse;
            }

            const virtualCSharpUri = razorDocument.csharpDocument.uri;

            const range = convertRangeFromSerializable(semanticTokensParams.range);

            const semanticTokens = await vscode.commands.executeCommand<vscode.SemanticTokens>(
                'vscode.provideDocumentSemanticTokens',
                virtualCSharpUri,
                range) as vscode.SemanticTokens | undefined;

            if (semanticTokens === undefined) {
                return this.emptySemanticTokensResponse;
            }

            return new ProvideSemanticTokensResponse(
                new SemanticTokensResponse(Array.from(semanticTokens.data), true, semanticTokens.resultId), true, null);
        } catch (error) {
            this.logger.logWarning(`${SemanticTokensHandler.getSemanticTokensEndpoint} failed with ${error}`);
        }

        return this.emptySemanticTokensResponse;
    }
}
