# Jira BTS 자동화 스킬

## 개요
이 스킬은 BTS 프로젝트(Software Development Project)의 Jira 이슈를 Claude를 통해 대화형으로 관리하는 방법을 정의합니다.

**⚠️ 중요: BTS 프로젝트만 사용. UMS, PTM, TC 프로젝트는 절대 건드리지 않음.**

## 기본 설정
- **Cloud ID:** `df2de822-077d-4a4b-843c-76307c49165d`
- **Project Key:** `BTS`
- **Jira URL:** `https://rsarndsw.atlassian.net`

## 이슈 타입
| 타입 | 용도 |
|---|---|
| 에픽 | 큰 기능 단위, Phase 묶음 |
| 버그 | 문제/오류 |
| 작업 | 개별 개발 태스크 |
| 새 기능 | 신규 기능 개발 |
| 개선 | 기존 기능 개선 |

## 이슈 요약 네이밍 규칙
형식: `{약어} - {전체명} {상세내용} / Phase {N}`

- **약어를 제일 앞에** 배치 (2~3글자, 대문자)
- 약어 뒤에 전체 Phase명 + 상세 내용 기재
- RSWare 프로젝트명은 제외
- 버그: `[BUG] {컴포넌트} - {증상 요약}`

### 약어 매핑 테이블
| 전체 이름 | 약어 |
|---|---|
| Design Review | DR |
| Alpha Test | AT |
| Beta Test | BT |
| PoC Release | POC |
| New Feature | NF |
| Improvement | IMP |
| Bug Fix | BF |

### 예시
- `DR - Design Review 3 Samples / Phase 1`
- `AT - Alpha Test Iteration 1 / Phase 2`
- `AT - Alpha Test Iteration 2 / Phase 3`
- `BT - Beta Test / Phase 4`
- `POC - PoC Demo S/W Release / Phase 5`

## 사용 예시 명령어

### 이슈 생성
```
BTS에 EtherCAT 통신 타임아웃 버그 이슈 만들어줘
BTS에 CSD7 파라미터 저장 기능 새 기능 이슈 추가해줘
BTS-38 에픽 아래에 UI 레이아웃 작업 이슈 만들어줘
```

### 상태 변경
```
BTS-39 진행 중으로 변경해줘
BTS-40 완료로 바꿔줘
BTS-38부터 BTS-43까지 전부 진행 중으로 변경해줘
```

### 댓글 추가
```
BTS-39에 "디자인 리뷰 완료, 샘플 3개 승인됨" 댓글 달아줘
BTS-40에 오늘 진행 상황 댓글로 남겨줘
```

### 워크로그 추가
```
BTS-39에 오늘 작업한 3시간 워크로그 추가해줘
BTS-40에 2h 30m 워크로그 기록해줘
```

### 조회
```
BTS 열린 이슈 전부 보여줘
BTS에서 내가 담당한 이슈 목록 알려줘
BTS 버그 이슈만 조회해줘
BTS-38 에픽 하위 이슈 목록 보여줘
```

### 마일스톤 일괄 등록
```
아래 마일스톤 BTS에 등록해줘:
1. Design Review - 3 Samples - 03/12
2. Alpha Test - Iteration 1 - 04/08
```

## 현재 등록된 마일스톤 (2026)
| 이슈 | Phase | Task | Target Date |
|---|---|---|---|
| BTS-39 | Design Review | 3 Samples | ~03/12 |
| BTS-40 | Alpha Test | Iteration 1 | ~04/08 |
| BTS-41 | Alpha Test | Iteration 2 | ~04/15 |
| BTS-42 | Beta Test | Beta Test | ~04/22 |
| BTS-43 | PoC Release | PoC Demo S/W Release | ~04/24 |

## Claude Code CLI 로컬 사용법

### 스킬 파일 배치
이 파일을 프로젝트 루트 또는 Claude Code 스킬 디렉토리에 배치:
```
{프로젝트루트}/.claude/skills/jira-bts/SKILL.md
```

### Claude Code CLI에서 호출
```bash
# 이슈 생성
claude "BTS에 서보 모터 파라미터 로드 실패 버그 이슈 만들어줘"

# 상태 변경
claude "BTS-39 완료로 변경해줘"

# 조회
claude "BTS 이번 주 마감 이슈 목록 보여줘"
```

### CLAUDE.md에 스킬 참조 추가
프로젝트 루트의 `CLAUDE.md`에 아래 내용 추가:
```markdown
## Jira 자동화
Jira BTS 프로젝트 관리는 .claude/skills/jira-bts/SKILL.md 스킬을 참조.
BTS 프로젝트(Software Development Project)만 사용하며 다른 프로젝트는 건드리지 않음.
```

## 주의사항
- 이슈 삭제는 Claude로 하지 않고 Jira UI에서 직접 처리
- 담당자 변경 시 accountId 필요 (기본값: 최봉철 `712020:b387febc-99d1-4b0b-96d0-f6b8125be28a`)
- 에픽 연결 시 parent 필드에 에픽 이슈 ID 사용
