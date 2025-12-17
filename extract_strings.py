#!/usr/bin/env python3
# -*- coding: utf-8 -*-

import os
import re
from pathlib import Path

# 프로젝트 경로
project_root = r"f:\GitHub\Puang-Adventure\Assets\Scripts"

# C# 문자열 리터럴 패턴 (정규식)
# 1. 일반 문자열: "..."
# 2. Verbatim 문자열: @"..."
# 3. 보간 문자열: $"..." 또는 $@"..."
string_pattern = re.compile(
    r'(?:@"(?:[^"]|"")*")|'  # Verbatim 문자열 @"..."
    r'(?:\$@"(?:[^"]|"")*")|'  # Verbatim 보간 문자열 $@"..."
    r'(?:\$"(?:[^"\\]|\\.)* ")|'  # 보간 문자열 $"..."
    r'(?:"(?:[^"\\]|\\.|\\u[0-9a-fA-F]{4})*")'  # 일반 문자열 "..."
)

# 한글 검출 패턴
korean_pattern = re.compile(r'[가-힣]')

# 주석 제거를 위한 패턴
comment_pattern = re.compile(r'//.*?$|/\*.*?\*/', re.MULTILINE | re.DOTALL)

def extract_strings_from_file(file_path):
    """C# 파일에서 문자열 리터럴 추출"""
    strings = []

    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()

        # 주석 제거
        content_without_comments = comment_pattern.sub('', content)

        # 문자열 추출
        matches = string_pattern.findall(content_without_comments)

        for match in matches:
            # 따옴표 제거 및 이스케이프 시퀀스 처리
            cleaned = match.strip()

            # @"..." 형식 처리
            if cleaned.startswith('@"') and cleaned.endswith('"'):
                cleaned = cleaned[2:-1].replace('""', '"')
            # $@"..." 형식 처리
            elif cleaned.startswith('$@"') and cleaned.endswith('"'):
                cleaned = cleaned[3:-1].replace('""', '"')
            # $"..." 형식 처리
            elif cleaned.startswith('$"') and cleaned.endswith('"'):
                cleaned = cleaned[2:-1]
            # "..." 형식 처리
            elif cleaned.startswith('"') and cleaned.endswith('"'):
                cleaned = cleaned[1:-1]

            # 빈 문자열 제외
            if cleaned.strip():
                strings.append(cleaned)

    except Exception as e:
        print(f"Error reading {file_path}: {e}")

    return strings

def main():
    """메인 실행 함수"""
    import sys

    # UTF-8 출력 설정 (Windows 콘솔 인코딩 문제 해결)
    if sys.platform == 'win32':
        import codecs
        sys.stdout = codecs.getwriter('utf-8')(sys.stdout.buffer, 'strict')

    all_strings = set()  # 중복 제거를 위해 set 사용
    korean_strings = set()
    other_strings = set()

    # 모든 .cs 파일 검색
    cs_files = list(Path(project_root).rglob("*.cs"))

    print(f"총 {len(cs_files)}개의 C# 파일 검색 중...\n")

    for cs_file in cs_files:
        strings = extract_strings_from_file(cs_file)

        for s in strings:
            # 한글 포함 여부 확인
            if korean_pattern.search(s):
                korean_strings.add(s)
            else:
                other_strings.add(s)

            all_strings.add(s)

    # 정렬
    korean_strings_sorted = sorted(korean_strings)
    other_strings_sorted = sorted(other_strings)

    # 결과 출력
    print(f"=== 한글 문자열 (총 {len(korean_strings_sorted)}개) ===\n")
    for s in korean_strings_sorted:
        print(s)

    print(f"\n\n=== 영어/기타 문자열 (총 {len(other_strings_sorted)}개) ===\n")
    for s in other_strings_sorted:
        print(s)

    print(f"\n\n=== 통계 ===")
    print(f"총 고유 문자열 수: {len(all_strings)}")
    print(f"한글 문자열: {len(korean_strings_sorted)}")
    print(f"영어/기타 문자열: {len(other_strings_sorted)}")

if __name__ == "__main__":
    main()
