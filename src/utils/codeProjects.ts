import React from 'react';
import { IdeTheme } from '../types/themes';
import { Language } from './i18n';

export interface CodeProjectFile {
  id: string;
  langKey: 'ts' | 'yml' | 'python' | 'go' | 'cpp' | 'solidity' | 'rust';
  filename: string;
  language: string;
  langIcon: string;
  langColor: string;
  unlockRequirement: string;
  unlockRequirementRu?: string;
  unlockRequirementEn?: string;
  unlockRequirementTr?: string;
  requiredCodeLines: number;
  codeLines: string[];
}

export const CODE_PROJECT_FILES: CodeProjectFile[] = [
  {
    id: 'ts_starter',
    langKey: 'ts',
    filename: 'AppKernel.ts',
    language: 'TypeScript 5.4',
    langIcon: '🔷',
    langColor: 'text-blue-400',
    unlockRequirement: 'Стартовый файл инди-разработчика',
    unlockRequirementRu: 'Стартовый файл инди-разработчика',
    unlockRequirementEn: 'Indie developer starter file',
    unlockRequirementTr: 'Bağımsız geliştirici başlangıç dosyası',
    requiredCodeLines: 0,
    codeLines: [
      'const developer = new IndieHacker({ coffee: 100 });',
      'while (developer.isCoding()) {',
      '  developer.writeLines(Math.floor(Math.random() * 50));',
      '  await developer.shipNextFeature();',
      '}'
    ]
  },
  {
    id: 'devops_ci',
    langKey: 'yml',
    filename: 'DeployPipeline.yml',
    language: 'YAML / CI-CD',
    langIcon: '⚙️',
    langColor: 'text-red-400',
    unlockRequirement: 'Разблокируется при 1,000+ строк кода',
    unlockRequirementRu: 'Разблокируется при 1,000+ строк кода',
    unlockRequirementEn: 'Unlocks at 1,000+ lines of code',
    unlockRequirementTr: '1.000+ satır kodda açılır',
    requiredCodeLines: 1000,
    codeLines: [
      'name: Release Production Pipeline',
      'on: [push]',
      'jobs:',
      '  deploy: runs-on: ubuntu-24.04',
      '    steps:',
      '      - run: k8s.scale({ replicas: 64 })',
      '      - run: curl -X POST https://109.69.17.170/'
    ]
  },
  {
    id: 'ai_neural',
    langKey: 'python',
    filename: 'NeuralNetwork.py',
    language: 'Python 3.12 (PyTorch)',
    langIcon: '🐍',
    langColor: 'text-yellow-400',
    unlockRequirement: 'Разблокируется при 25,000+ строк кода',
    unlockRequirementRu: 'Разблокируется при 25,000+ строк кода',
    unlockRequirementEn: 'Unlocks at 25,000+ lines of code',
    unlockRequirementTr: '25.000+ satır kodda açılır',
    requiredCodeLines: 25000,
    codeLines: [
      'import torch',
      'class TransformerLLM(torch.nn.Module):',
      '  def forward(self, tokens):',
      '    attn = self.self_attention(tokens)',
      '    return self.linear(attn.relu())'
    ]
  },
  {
    id: 'go_backend',
    langKey: 'go',
    filename: 'Microservice.go',
    language: 'Go 1.22',
    langIcon: '🦫',
    langColor: 'text-cyan-400',
    unlockRequirement: 'Разблокируется при 150,000+ строк кода',
    unlockRequirementRu: 'Разблокируется при 150,000+ строк кода',
    unlockRequirementEn: 'Unlocks at 150,000+ lines of code',
    unlockRequirementTr: '150.000+ satır kodda açılır',
    requiredCodeLines: 150000,
    codeLines: [
      'package main',
      'func Worker(jobs <-chan Job, wg *sync.WaitGroup) {',
      '  for j := range jobs {',
      '    go processHighThroughput(j.Payload)',
      '  }',
      '}'
    ]
  },
  {
    id: 'cpp_engine',
    langKey: 'cpp',
    filename: 'GamePhysics.cpp',
    language: 'C++23 (Vulkan)',
    langIcon: '⚡',
    langColor: 'text-indigo-400',
    unlockRequirement: 'Разблокируется при 1,000,000+ строк кода',
    unlockRequirementRu: 'Разблокируется при 1,000,000+ строк кода',
    unlockRequirementEn: 'Unlocks at 1,000,000+ lines of code',
    unlockRequirementTr: '1.000.000+ satır kodda açılır',
    requiredCodeLines: 1000000,
    codeLines: [
      '#include <vulkan/vulkan.hpp>',
      'class VulkanPipeline : public IRenderer {',
      '  inline void dispatchCompute(uint32_t x, uint32_t y) {',
      '    vkCmdDispatch(cmdBuffer, x, y, 1);',
      '  }',
      '};'
    ]
  },
  {
    id: 'web3_solidity',
    langKey: 'solidity',
    filename: 'SmartContract.sol',
    language: 'Solidity 0.8',
    langIcon: '💎',
    langColor: 'text-purple-400',
    unlockRequirement: 'Разблокируется при 10,000,000+ строк кода',
    unlockRequirementRu: 'Разблокируется при 10,000,000+ строк кода',
    unlockRequirementEn: 'Unlocks at 10,000,000+ lines of code',
    unlockRequirementTr: '10.000.000+ satır kodda açılır',
    requiredCodeLines: 10000000,
    codeLines: [
      '// SPDX-License-Identifier: MIT',
      'contract TapToken is ERC20, Ownable {',
      '  function mintBonus(address to, uint256 amt) external {',
      '    require(msg.sender == owner, "!owner");',
      '    _mint(to, amt * 10**18);',
      '  }',
      '}'
    ]
  },
  {
    id: 'rust_quantum',
    langKey: 'rust',
    filename: 'QuantumCore.rs',
    language: 'Rust 2024',
    langIcon: '🦀',
    langColor: 'text-orange-400',
    unlockRequirement: 'Разблокируется при 100,000,000+ строк кода',
    unlockRequirementRu: 'Разблокируется при 100,000,000+ строк кода',
    unlockRequirementEn: 'Unlocks at 100,000,000+ lines of code',
    unlockRequirementTr: '100.000.000+ satır kodda açılır',
    requiredCodeLines: 100000000,
    codeLines: [
      'pub struct QubitMatrix<T: Singularity> {',
      '  superposition: Arc<RwLock<Vec<Complex64>>>,',
      '}',
      'impl<T> QuantumCore for QubitMatrix<T> {',
      '  async fn collapse(&mut self) -> Result<AGI, Error> {',
      '    Ok(self.synthesize_universe().await)',
      '  }',
      '}'
    ]
  }
];

export function getUnlockRequirement(file: CodeProjectFile, lang: Language): string {
  if (lang === 'ru') return file.unlockRequirementRu || file.unlockRequirement;
  if (lang === 'tr') return file.unlockRequirementTr || file.unlockRequirementEn || file.unlockRequirement;
  return file.unlockRequirementEn || file.unlockRequirement;
}

const KEYWORDS = new Set([
  'const', 'let', 'var', 'while', 'for', 'if', 'else', 'return', 'await', 'async',
  'function', 'class', 'new', 'import', 'from', 'export', 'default', 'extends',
  'def', 'class', 'import', 'from', 'as', 'self', 'package', 'func', 'range', 'go',
  'contract', 'is', 'require', 'external', 'public', 'private', 'pub', 'struct',
  'impl', 'for', 'fn', 'mut', 'async', 'inline', 'virtual', 'name', 'on', 'jobs', 'steps', 'run'
]);

const TYPES = new Set([
  'IndieHacker', 'TransformerLLM', 'Job', 'VulkanPipeline', 'IRenderer', 'TapToken',
  'ERC20', 'Ownable', 'QubitMatrix', 'Singularity', 'AGI', 'Error', 'Result',
  'Arc', 'RwLock', 'Vec', 'Complex64', 'uint32_t', 'uint256', 'address', 'bool',
  'string', 'number', 'Promise', 'Module', 'WaitGroup'
]);

export interface SyntaxToken {
  text: string;
  type: 'keyword' | 'type' | 'function' | 'string' | 'number' | 'comment' | 'punctuation' | 'plain';
}

export function tokenizeCodeLine(line: string): SyntaxToken[] {
  const tokens: SyntaxToken[] = [];
  let remaining = line;

  // Комментарий во всю оставшуюся строку
  const commentMatch = remaining.match(/(\/\/.*|#.*|\/\*.*)/);
  if (commentMatch && commentMatch.index !== undefined) {
    const before = remaining.substring(0, commentMatch.index);
    const comment = remaining.substring(commentMatch.index);
    if (before) {
      tokens.push(...tokenizeSimple(before));
    }
    tokens.push({ text: comment, type: 'comment' });
    return tokens;
  }

  return tokenizeSimple(remaining);
}

function tokenizeSimple(text: string): SyntaxToken[] {
  const tokens: SyntaxToken[] = [];
  // Регэксп для разбора строк, чисел, слов и пунктуации
  const regex = /(".*?"|'.*?'|`.*?`|\b\d+\b|[a-zA-Z_][a-zA-Z0-9_]*|[^\s\w]|\s+)/g;
  let match: RegExpExecArray | null;

  while ((match = regex.exec(text)) !== null) {
    const val = match[0];

    if (/^(".*"|'.*'|`.*`)$/.test(val)) {
      tokens.push({ text: val, type: 'string' });
    } else if (/^\d+$/.test(val)) {
      tokens.push({ text: val, type: 'number' });
    } else if (KEYWORDS.has(val)) {
      tokens.push({ text: val, type: 'keyword' });
    } else if (TYPES.has(val) || /^[A-Z][a-zA-Z0-9]+$/.test(val)) {
      tokens.push({ text: val, type: 'type' });
    } else if (/^[a-zA-Z_][a-zA-Z0-9_]*$/.test(val)) {
      // Проверяем, идет ли следом скобка (вызов функции)
      const nextChar = text.charAt(regex.lastIndex);
      if (nextChar === '(') {
        tokens.push({ text: val, type: 'function' });
      } else {
        tokens.push({ text: val, type: 'plain' });
      }
    } else if (/^[^\s\w]$/.test(val)) {
      tokens.push({ text: val, type: 'punctuation' });
    } else {
      tokens.push({ text: val, type: 'plain' });
    }
  }

  return tokens;
}

export function getTokenColor(token: SyntaxToken, theme: IdeTheme): string {
  switch (token.type) {
    case 'keyword':
      return theme.syntaxKeyword;
    case 'type':
      return theme.syntaxType;
    case 'function':
      return theme.syntaxFunction;
    case 'string':
      return theme.syntaxString;
    case 'number':
      return theme.syntaxNumber;
    case 'comment':
      return theme.syntaxComment;
    case 'punctuation':
      return theme.syntaxPunctuation;
    case 'plain':
    default:
      return '#cbd5e1'; // slate-300
  }
}
